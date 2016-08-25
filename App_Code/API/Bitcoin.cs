using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for Bitcoin
/// </summary>

/// <summery>
/// Manage Bitcoins
/// </summery>
class Bitcoin {

    var COUNT_PER_PAGE = 20;

    /// <summery>
    /// Create Wallet Address
    /// 
    /// <typeparam name=""></typeparam> Int userID
    /// <returns></returns> array|bool
    /// </summery>
    public static function createWallet(userID, userEmail){
        global db;

        password = generate_random_string(10);

        ch = curl_init();
        curl_setopt(ch, CURLOPT_URL, "https://blockchain.info/api/v2/create_wallet?api_code=" + BLOCKCHAIN_INFO_API_KEY + "&password=" + password);
        curl_setopt(ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt(ch, CURLOPT_SSL_VERIFYPEER, false);

        return = curl_exec(ch);
        curl_close(ch);

        returnData = json_decode(return);

        if(!returnData){
            add_message("There was an error to create bitcoin account: " + return, "error");
            return false;
        }

        data = ["userID" => userID, "bitcoin_guid" => returnData.guid, "bitcoin_address" => returnData.address, "bitcoin_link" => returnData.link, "bitcoin_password" => encrypt(password)];

        db.insertFromArray(TABLE_USERS_BITCOIN, data);

        return data;
    }

    /// <summery>
    /// Get balance
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// <returns></returns> bool|float|int
    /// </summery>
    public static function getUserWalletBalance(userID){
        bitcoinInfo = User.getUserBitcoinInfo(userID);

        if(!bitcoinInfo)
            return 0;

        balance = Bitcoin.getWalletBalance(bitcoinInfo["bitcoin_guid"], decrypt(bitcoinInfo["bitcoin_password"]));

        return balance;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> guid
    /// <typeparam name=""></typeparam> password
    /// <returns></returns> bool|float|int
    /// </summery>
    public function getWalletBalance(guid, password){
        global db;

        ch = curl_init();
        curl_setopt(ch, CURLOPT_URL, "https://blockchain.info/merchant/" + guid + "/balance?password=" + password);

        curl_setopt(ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt(ch, CURLOPT_SSL_VERIFYPEER, false);

        return = curl_exec(ch);

        curl_close(ch);

        returnData = json_decode(return);

        if(!returnData){
            add_message("There was an error to get balance of your wallet: " + return, MSG_TYPE_ERROR);
            return false;
        }

        if(isset(returnData.error)){
            add_message("Getting Balance Error: " + returnData.error, MSG_TYPE_ERROR);
            return 0;
        }

        return returnData.balance / 100000000;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <typeparam name=""></typeparam> toAddress
    /// <typeparam name=""></typeparam> amount
    /// <returns></returns> bool
    /// </summery>
    public static function sendBitcoin(userID, toAddress, amount){
        global db;

        bitcoinInfo = User.getUserBitcoinInfo(userID);

        amount = amount * 100000000;

        ch = curl_init();
        curl_setopt(ch, CURLOPT_URL, "https://blockchain.info/merchant/" + bitcoinInfo["bitcoin_guid"] + "/payment?password=" + decrypt(bitcoinInfo["bitcoin_password"]) + "&to=" + toAddress + "&amount=" + amount + "&from=" + bitcoinInfo["bitcoin_address"]);

        curl_setopt(ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt(ch, CURLOPT_SSL_VERIFYPEER, false);

        return = curl_exec(ch);

        curl_close(ch);

        returnData = json_decode(return);

        if(!returnData){
            add_message("There was an error to send the bitcoin: " + return, MSG_TYPE_ERROR);
            return false;
        }

        if(isset(returnData.error)){
            add_message("Sending Bitcoin Error: " + returnData.error, MSG_TYPE_ERROR);
            return false;

        }else{
            add_message(returnData.message);
            if(isset(returnData.notice) && returnData.notice)
                add_message(returnData.notice, MSG_TYPE_NOTICE);
        }

        return true;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> address
    /// <typeparam name=""></typeparam> amount
    /// <returns></returns> bool
    /// </summery>
    public function sendBitcoinFromroom(address, amount){
        global db;

        amount = amount * 100000000;

        ch = curl_init();
        curl_setopt(ch, CURLOPT_URL, "https://blockchain.info/merchant/" + BITCOIN_GUID + "/payment?password=" + BITCOIN_PASSWORD + "&to=" + address + "&amount=" + amount + "&from=" + BITCOIN_ADDRESS);

        curl_setopt(ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt(ch, CURLOPT_SSL_VERIFYPEER, false);

        return = curl_exec(ch);

        curl_close(ch);

        returnData = json_decode(return);

        if(!returnData){
            add_message("There was an error to send the bitcoin: " + return, MSG_TYPE_ERROR);
            return false;
        }

        if(isset(returnData.error)){
            add_message("Sending Bitcoin Error: " + returnData.error, MSG_TYPE_ERROR);
            return false;

        }else{
            add_message(returnData.message);
            if(isset(returnData.notice) && returnData.notice)
                add_message(returnData.notice, MSG_TYPE_NOTICE);
        }

        return true;
    }

    /// <summery>
    /// <typeparam name=""></typeparam>     userID
    /// <typeparam name=""></typeparam> int page
    /// <typeparam name=""></typeparam> int limit
    /// <returns></returns> array|bool
    /// </summery>
    public function getTransactions(userID, page = 1, limit = 20){
        global db;

        if(!this._getTransactions(userID)){
            return false;
        }

        //Getting Total Transactions and final balance
        query = db.prepare("SELECT count(*) AS n_tx FROM " + TABLE_USERS_BITCOIN_TRANSACTIONS_HISTORY + " WHERE userID=%d", userID);
        totalCount = db.getVar(query);

        query = db.prepare("SELECT balance FROM " + TABLE_USERS_BITCOIN_TRANSACTIONS_HISTORY + " WHERE userID=%d ORDER BY `date` DESC LIMIT 1", userID);
        finalBalance = db.getVar(query);

        //Getting Transactions
        query = db.prepare("SELECT t.addr, t.type, t.amount, t.balance, t.date FROM " + TABLE_USERS_BITCOIN_TRANSACTIONS_HISTORY + " t                                
                               WHERE t.userID=%d ORDER BY t.date DESC LIMIT %d, %d", userID, (page - 1) * limit, limit);

        data = db.getResultsArray(query);

        return [totalCount, finalBalance, data];

    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <returns></returns> bool
    /// </summery>
    private function _getTransactions(userID){
        global db;

        bitcoinInfo = User.getUserBitcoinInfo(userID);

        //Getting User Last Transaction
        query = db.prepare("SELECT * FROM " + TABLE_USERS_BITCOIN_TRANSACTIONS_HISTORY + " WHERE userID=%d ORDER BY `date` DESC", userID);
        lastTrans = db.getRow(query);

        limit = 20;
        offset = 0;

        do{
            ch = curl_init();

            curl_setopt(ch, CURLOPT_RETURNTRANSFER, true);
            curl_setopt(ch, CURLOPT_SSL_VERIFYPEER, false);
            curl_setopt(ch, CURLOPT_URL, "https://blockchain.info/address/" + bitcoinInfo["bitcoin_address"] + "?format=json&limit=" + limit + "&offset=" + offset);
            return = curl_exec(ch);

            curl_close(ch);

            returnData = json_decode(return);

            if(!returnData){
                add_message("There was an error to get transactions: " + return, MSG_TYPE_ERROR);
                return false;
            }

            if(isset(returnData.error)){
                add_message("There was an error to get transactions: " + returnData.error, MSG_TYPE_ERROR);
                return false;

            }else{
                transactions = returnData.txs;

                if(!transactions){
                    this.fixBalances(userID, !lastTrans ? 0.0 : lastTrans["balance"]);
                    return true;
                }

                foreach(transactions as trx){
                    if(lastTrans && lastTrans["hash"] == trx.hash){
                        this.fixBalances(userID, !lastTrans ? 0.0 : lastTrans["balance"]);
                        return true;
                    }

                    row = [];

                    row["userID"] = userID;
                    row["hash"] = trx.hash;
                    row["date"] = trx.time;
                    row["balance"] = -1.0;
                    row["addr"] = [];
                    row["amount"] = [];
                    row["totalAmount"] = 0;

                    if(trx.inputs[0].prev_out.addr != bitcoinInfo["bitcoin_address"]) //Received
                    {
                        row["addr"][] = trx.inputs[0].prev_out.addr;

                        foreach(trx.out as out){
                            if(out.addr == bitcoinInfo["bitcoin_address"]){
                                row["amount"][] = intval(out.value);
                                row["totalAmount"] += intval(out.value);
                            }
                        }

                        row["type"] = "received";
                    }else{
                        //Send Bitcoin
                        foreach(trx.out as out){
                            if(out.addr != bitcoinInfo["bitcoin_address"]){
                                row["addr"][] = out.addr;
                                row["amount"][] = -1 * intval(out.value);
                                row["totalAmount"] += intval(out.value);
                            }
                        }

                        if(!row["addr"]) //It means that user sent it to himeself
                        {
                            row["addr"][] = trx.out[0].addr;
                            row["amount"][] = -1 * intval(trx.out[0].value);
                            row["totalAmount"] += 0;
                        }

                        row["type"] = "sent";
                        row["totalAmount"] += ceil(trx.size / 1000) * 10000;
                    }

                    row["addr"] = implode("\n", row["addr"]);
                    row["amount"] = implode("\n", row["amount"]);

                    db.insertFromArray(TABLE_USERS_BITCOIN_TRANSACTIONS_HISTORY, row);

                }

                if(count(transactions) < limit){
                    this.fixBalances(userID, !lastTrans ? 0.0 : lastTrans["balance"]);
                    return true;
                }
            }

            offset += limit;
        }while(1);

        return true;
    }

    /// <summery>
    /// <typeparam name=""></typeparam>     userID
    /// <typeparam name=""></typeparam> int balance
    /// </summery>
    public function fixBalances(userID, balance = 0){
        global db;

        //Fix balanace
        //query = db.prepare("SELECT * FROM " + TABLE_USERS_BITCOIN_TRANSACTIONS_HISTORY + " WHERE userID=%d AND balance < 0 ORDER BY `date`", userID);
        query = db.prepare("SELECT * FROM " + TABLE_USERS_BITCOIN_TRANSACTIONS_HISTORY + " WHERE userID=%d ORDER BY `date` ASC", userID);
        rows = db.getResultsArray(query);

        balance = 0;

        foreach(rows as row){
            if(row["type"] == "sent")
                balance -= row["totalAmount"];else
                balance += row["totalAmount"];

            db.updateFromArray(TABLE_USERS_BITCOIN_TRANSACTIONS_HISTORY, ["balance" => balance], ["transactionID" => row["transactionID"]]);
        }
    }

    /// <summery>
    /// <typeparam name=""></typeparam> addr
    /// <returns></returns> array
    /// </summery>
    public function getUserInfoFromAddr(addr){
        global db;

        query = db.prepare("SELECT u.userID, u.firstName, u.lastName FROM " + TABLE_USERS_BITCOIN + " AS ub LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=ub.userID WHERE ub.bitcoin_address=%s", addr);

        row = db.getRow(query);

        return row;
    }

}