using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for Pagination
/// </summary>

class Pagination {

    var total = 0;
    var per_page = 0;
    var currrent_page = 1;
    var total_page = 0;

    /// <summery>
    /// Construct
    /// 
    /// <typeparam name=""></typeparam> Int total        = Total Item Numbers
    /// <typeparam name=""></typeparam> Int per_page     = Item Numbers Per a Page
    /// <typeparam name=""></typeparam> Int current_page = Current page
    /// </summery>
    public function __construct(total = 0, per_page = 0, current_page = 0){
        if(total < 1 || per_page < 1)
            return;
        this.total = total;
        this.per_page = per_page;
        this.total_page = ceil(this.total / this.per_page);
        if(current_page < 1)
            this.currrent_page = 1;else if(current_page > this.total_page)
            this.currrent_page = this.total_page;else
            this.currrent_page = current_page;

        return;
    }

    /// <summery>
    /// <returns></returns> int
    /// </summery>
    public function getCurrentPage(){
        return this.currrent_page;
    }

    /// <summery>
    /// <returns></returns> float|int
    /// </summery>
    public function getTotalPage(){
        return this.total_page;
    }

    /// <summery>
    /// <returns></returns> int
    /// </summery>
    public function getTotalItems(){
        return this.total;
    }

    /// <summery>
    /// Display Pagination Bar
    /// 
    /// <typeparam name=""></typeparam> mixed isReturn
    /// <returns></returns> string
    /// </summery>
    public function renderPaginate(mainURL = "", current_items = 0, isReturn = false){
        ob_start();
        if(this.total_page > 0){

            start = (this.currrent_page - 1) * this.per_page + 1;
            end = start + current_items - 1;
            ?>
            <div class="paginate">
                <span
                    class="label"> echo number_format(start) ?> -  echo number_format(end)?> of  echo this.total?></span>

                <div class="pages">
                    
                    sPage = current_items - 2;

                    startItems = 2;
                    centerItems = this.total_page > 5 ? 5 : this.total_page;
                    endItems = 2;

                    cStart = this.currrent_page - 2;
                    cEnd = this.currrent_page + 2;
                    while(cStart <= 0){
                        cStart++;
                        cEnd++;
                    }

                    while(cStart > 1 && cEnd > this.total_page){
                        cStart--;
                        cEnd--;
                    }

                    if(cEnd > this.total_page)
                        cEnd = this.total_page;

                    if(cStart < 3)
                        startItems = 2 - (3 - cStart);

                    if(this.total_page - cEnd < 3)
                        endItems = this.total_page - cEnd;

                    //Show Prev
                    if(this.total_page > 1){
                        if(this.currrent_page > 1){
                            ?>
                            <a href=" echo mainURL?>page= echo this.currrent_page - 1?>">Prev</a>
                        
                        }else{
                            ?>
                            <span class="current">Prev</span>
                        
                        }
                    }

                    for(i = 1; i <= startItems; i++){
                        ?>
                        <a href=" echo mainURL?>page= echo i?>"> echo i?></a>
                    

                    }
                    //Show Ellipse
                    if(startItems == 2 && cStart > 3){
                        if(cStart - startItems > 2){
                            ?>
                            <span class="ellipse">...</span>
                        
                        }else{
                            ?>
                            <a href=" echo mainURL?>page= echo startItems + 1?>"> echo startItems + 1?></a>
                        
                        }
                    }

                    for(i = cStart; i <= cEnd; i++){
                        if(i == this.currrent_page){
                            ?>
                            <span class="current"> echo i?></span>
                        
                        }else{
                            ?>
                            <a href=" echo mainURL?>page= echo i?>"> echo i?></a>
                        
                        }

                    }

                    //Show Ellipse
                    if(endItems == 2){
                        if(this.total_page - cEnd > 3){
                            ?>
                            <span class="ellipse">...</span>
                        
                        }else if(this.total_page - cEnd == 3){
                            ?>
                            <a href=" echo mainURL ?>page= echo this.total_page - 2 ?>"> echo this.total_page - 2 ?></a>
                        
                        }
                    }
                    for(i = endItems - 1; i >= 0; i--){
                        ?>
                        <a href=" echo mainURL?>page= echo this.total_page - i?>"> echo this.total_page - i?></a>
                    

                    }
                    //Show Next
                    if(this.total_page > 1){
                        if(this.currrent_page < this.total_page){
                            ?>
                            <a href=" echo mainURL?>page= echo this.currrent_page + 1?>">Next</a>
                        
                        }else{
                            ?>
                            <span class="current">Next</span>
                        
                        }
                    }
                    ?>
                </div>
                <div class="clear"></div>
            </div>
        
        }
        html = ob_get_contents();
        ob_end_clean();
        if(isReturn)
            return html;else
            echo html;
    }
}