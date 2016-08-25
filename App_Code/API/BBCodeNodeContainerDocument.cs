using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for BBCodeNodeContainerDocument
/// </summary>

class BBCodeNodeContainerDocument extends BBCodeNodeContainer {

    /// <summery>
    /// Current Tag
    /// 
    /// @var BBCodeNodeContainer
    /// </summery>
    protected current_tag = null;

    /// <summery>
    /// Assoc array of all the BBCodes for this document
    /// 
    /// @var array
    /// </summery>
    protected bbcodes = [];

    /// <summery>
    /// Base URI to be used for links, images, ect.
    /// 
    /// @var string
    /// </summery>
    protected base_uri = null;

    /// <summery>
    /// Assoc array of emoticons in smiley_code => smiley_url format
    /// 
    /// @var array
    /// </summery>
    protected emoticons = [];

    /// <summery>
    /// If to throw errors when encountering bad BBCode or
    /// to silently try and fix
    /// @var bool
    /// </summery>
    protected throw_errors = false;

    public function __construct(load_defaults = true, throw_errors = false){
        this.throw_errors = throw_errors;

        // Load default BBCodes
        if(load_defaults){
            this.add_bbcodes(this.default_bbcodes());
            this.add_emoticons(this.default_emoticons());
        }
    }

    /// <summery>
    /// Gets an array of the default BBCodes
    /// 
    /// <returns></returns> array
    /// </summery>
    public static function default_bbcodes(){
        return [new BBCode("b", "<strong>%content%</strong>"), new BBCode("i", "<em>%content%</em>"), new BBCode("strong", "<strong>%content%</strong>"), new BBCode("em", "<em>%content%</em>"), new BBCode("u", "<span style="text-decoration: underline">%content%</span>"), new BBCode("s", "<span style="text-decoration: line-through">%content%</span>"), new BBCode("blink", "<span style="text-decoration: blink">%content%</span>"), new BBCode("sub", "<sub>%content%</sub>"), new BBCode("sup", "<sup>%content%</sup>"), new BBCode("ins", "<ins>%content%</ins>"), new BBCode("del", "<del>%content%</del>"),

            new BBCode("right", "<div style="text-align: right">%content%</div>", BBCode.BLOCK_TAG), new BBCode("left", "<div style="text-align: left">%content%</div>", BBCode.BLOCK_TAG), new BBCode("center", "<div style="text-align: center">%content%</div>", BBCode.BLOCK_TAG), new BBCode("justify", "<div style="text-align: justify">%content%</div>", BBCode.BLOCK_TAG),

            // notes only show in editing so ignore it
            new BBCode("note", ""), new BBCode("hidden", ""),

            new BBCode("abbr", function (content, attribs){
                return "<abbr title="" + attribs["default"] + "">" + content + "</abbr>";
            }), new BBCode("acronym", function (content, attribs){
                return "<acronym title="" + attribs["default"] + "">" + content + "</acronym>";
            }),

            new BBCode("icq", "<a href="http://www.icq.com/people/about_me.php?uin=%content%">
                <img  src="http://status.icq.com/online.gif?icq=%content%&amp;img=5"> %content%</a>", BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL), new BBCode("skype", "<a href="skype:jovisa737590?call">
                <img src="http://mystatus.skype.com/bigclassic/%content%" style="border: none;" width="182"
                height="44" alt="My status" /></a>", BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL),

            new BBCode("bing", "<a href="http://www.bing.com/search?q=%content%">%content%</a>", BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL), new BBCode("google", "<a href="http://www.google.com/search?q=%content%">%content%</a>", BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL), new BBCode("wikipedia", "<a href="http://www.wikipedia.org/wiki/%content%">%content%</a>", BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL), new BBCode("youtube", function (content, attribs){
                if(substr(content, 0, 23) === "http://www.youtube.com/")
                    uri = content;else
                    uri = "https://www.youtube.com/v/" + content;

                return "<iframe width="640" height="360" src="" + uri + "" frameborder="0"></iframe>";
            }, BBCode.BLOCK_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL), new BBCode("vimeo", function (content, attribs){
                if(substr(content, 0, 24) === "http://player.vimeo.com/")
                    uri = content;else if(substr(content, 0, 17) === "http://vimeo.com/" || substr(content, 0, 21) === "http://www.vimeo.com/" && preg_match("/http:\/\/(?:www\.)?vimeo\.com\/([0-9]{4,10})/", content, matches)){
                    preg_match("/http:\/\/(?:www\.)?vimeo\.com\/([0-9]{4,10})/", content, matches);
                    uri = "http://player.vimeo.com/video/" + matches[1] + "?title=0&amp;byline=0&amp;portrait=0";
                }else
                    uri = "http://player.vimeo.com/video/" + content + "?title=0&amp;byline=0&amp;portrait=0";

                return "<iframe src="" + uri + "" width="400" height="225" frameborder="0"></iframe>";
            }, BBCode.BLOCK_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL), new BBCode("flash", function (content, attribs){
                width = 640;
                height = 385;

                if(substr(content, 0, 4) !== "http")
                    content = node.root().get_base_uri() + content;

                if(isset(attribs["width"]) && is_numeric(attribs["width"]))
                    width = attribs["width"];
                if(isset(attribs["height"]) && is_numeric(attribs["height"]))
                    height = attribs["height"];

                // for [flash=200,100] format
                if(!empty(attribs["default"])){
                    list(w, h) = explode(",", attribs["default"]);

                    if(w > 20 && is_numeric(w))
                        width = w;

                    if(h > 20 && is_numeric(h))
                        height = h;
                }

                return "<object width="" + width + "" height="" + height + "">
                        <param name="movie" value="" + content + ""></param>
                        <embed src="" + content + ""
                            type="application/x-shockwave-flash"
                            width="" + width + "" height="" + height + "">
                        </embed>
                    </object>";
            }, BBCode.BLOCK_TAG),

            new BBCode("paypal", function (content, attribs){
                content = urlencode(content);

                return "<a href="https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=" + content + "&lc=US&no_note=0&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHostedGuest">
                <img src="https://www.paypal.com/en_US/i/btn/x-click-but21.gif" border="0" name="submit" alt="PayPal - The safer, easier way to pay online!"/></a>";
            }, BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL),

            new BBCode("pastebin", function (content, attribs){
                if(!preg_match("/^[a-zA-Z0-9]/", content)){
                    preg_match("#http://pastebin.com/([a-zA-Z0-9]+)#", content, matches);
                    content = "";

                    if(isset(matches[1]))
                        content = matches[1];
                }

                return "<script src="http://pastebin.com/embed_js.php?i=" + content + ""></script>";
            }, BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL), new BBCode("gist", function (content, attribs){
                if(content != (string)intval(content)){
                    preg_match("#https://gist.github.com/([0-9]+)#", content, matches);
                    content = "";

                    if(isset(matches[1]))
                        content = matches[1];
                }else
                    content = intval(content);

                return "<script src="http://gist.github.com/" + content + ".js"></script>";
            }, BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL),

            new BBCode("twitter", "<a href="https://twitter.com/%content%"
                class="twitter-follow-button" data-show-count="false">Follow @%content%</a>
                <script src="http://platform.twitter.com/widgets.js" type="text/javascript"></script>", BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL), new BBCode("tweets", "<script src=\"http://widgets.twimg.com/j/2/widget.js\"></script>
                <script>
                new TWTR.Widget({
                    version: 2,
                    type: "profile",
                    rpp: 3,
                    interval: 6000,
                    width: 400,
                    height: 150,
                    theme: {
                        shell: {
                            background: "#333333",
                            color: "#ffffff"
                        },
                        tweets: {
                            background: "#000000",
                            color: "#ffffff",
                            links: "#4aed05"
                        }
                    },
                    features: {
                        scrollbar: false,
                        loop: false,
                        live: false,
                        hashtags: true,
                        timestamp: true,
                        avatars: false,
                        behavior: "all"
                    }
                }).render().setUser("%content%").start();
                </script>", BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL),

            new BBCode("googlemaps", "<iframe src="http://maps.google.com/maps?q=%content%&amp;output=embed"
                scrolling="no" width="100%" height="350" frameborder="0"></iframe>", BBCode.BLOCK_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL),

            new BBCode("pdf", "<iframe src="http://docs.google.com/gview?url=%content%&amp;embedded=true"
                width="100%" height="500" frameborder="0"></iframe>", BBCode.BLOCK_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL),

            new BBCode("scribd", function (content, attribs){
                if(!isset(attribs["id"]) || !(attribs["id"] = intval(attribs["id"])) > 1)
                    return "Invalid scribd ID.";

                if(!isset(attribs["key"]))
                    return "Missing scribd key.";

                return "<iframe src="http://www.scribd.com/embeds/" + attribs["id"] + "/content?start_page=1&view_mode=list&access_key=" + attribs["key"] + ""
                    data-auto-height="true" data-aspect-ratio="1" scrolling="no" width="100%"
                    height="500" frameborder="0"></iframe>
                    <script type="text/javascript">(function() {
                        var scribd = document.createElement("script");
                        scribd.type = "text/javascript";
                        scribd.async = true;
                        scribd.src = "http://www.scribd.com/javascripts/embed_code/inject.js";
                        var s = document.getElementsByTagName("script")[0];
                        s.parentNode.insertBefore(scribd, s);
                    })();</script>";
            }, BBCode.BLOCK_TAG, true, [], [], BBCode.AUTO_DETECT_EXCLUDE_ALL),

            new BBCode("spoiler", "<div class="spoiler" style="margin:5px 15px 15px 15px">
                <div style="margin:0 0 2px 0; font-weight:bold;">Spoiler:
                    <input type="button" value="Show"
                        onclick="if (this.value == \"Show\")
                            {
                                this.parentNode.parentNode.getElementsByTagName(\"div\")[1].getElementsByTagName(\"div\")[0].style.display = \"block\";
                                this.value = \"Hide\";
                            } else {
                                this.parentNode.parentNode.getElementsByTagName(\"div\")[1].getElementsByTagName(\"div\")[0].style.display = \"none\";
                                this.value = \"Show\";
                            }" />
                </div>
                <div style="margin:0; padding:6px; border:1px inset;">
                    <div style="display: none;">
                        %content%
                    </div>
                </div>
            </div>"),

            new BBCode("tt", "<span style="font-family: monospace">%content%</span>"),

            new BBCode("pre", function (content, attribs, node){
                content = "";
                foreach(node.children() as child)
                    content += child.get_html(false);

                return "<pre>{content}</pre>";
            }, BBCode.BLOCK_TAG), new BBCode("code", function (content, attribs, node){
                content = str_replace("<", "&lt;", content);
                return "<pre><code>" + content + "</code></pre>";
            }, BBCode.BLOCK_TAG, false, [], [], BBCode.AUTO_DETECT_EXCLUDE_EMOTICON), new BBCode("php", function (content, attribs, node){
                ob_start();
                highlight_string(node.get_text());
                content = ob_get_contents();
                ob_end_clean();

                return "<code class=\"php\">{content}</code>";
            }, BBCode.BLOCK_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_EMOTICON),

            new BBCode("quote", function (content, attribs, node){
                cite = "";
                if(!empty(attribs["default"]))
                    cite = "<cite>{attribs["default"]}:</cite>";

                if(node.find_parent_by_tag("quote") !== null)
                    return "</p><blockquote><p>{cite}{content}</p></blockquote><p>";

                return "<blockquote><p>{cite}{content}</p></blockquote>";
            }, BBCode.BLOCK_TAG),

            new BBCode("font", function (content, attribs){
                // Font can have letters, spaces, quotes, - and commas
                if(!isset(attribs["default"]) || !preg_match("/^([A-Za-z\"\",\- ]+)/", attribs["default"]))
                    attribs["default"] = "Arial";

                return "<span style="font-family: " + attribs["default"] + "">" + content + "</span>";
            }), new BBCode("size", function (content, attribs){
                size = "xx-small";

                /*
                Font tag sizes 1-7 should be:
                1 = xx-small
                2 = small
                3 = medium
                4 = large
                5 = x-large
                6 = xx-large
                7 = ?? in chrome it"s 48px
                */
                if(!isset(attribs["default"]))
                    size = "xx-small";else if(attribs["default"] == 2)
                    size = "small";else if(attribs["default"] == 3)
                    size = "medium";else if(attribs["default"] == 4)
                    size = "large";else if(attribs["default"] == 5)
                    size = "x-large";else if(attribs["default"] == 6)
                    size = "xx-large";else if(attribs["default"] == 7)
                    size = "48px";else if(attribs["default"][strlen(attribs["default"]) - 1] === "%" && is_numeric(substr(attribs["default"], 0, -1)))
                    size = attribs["default"];else{
                    if(!is_numeric(attribs["default"]))
                        attribs["default"] = 13;
                    if(attribs["default"] < 6)
                        attribs["default"] = 6;
                    if(attribs["default"] > 48)
                        attribs["default"] = 48;

                    size = attribs["default"] + "px";
                }

                return "<span style="font-size: " + size + "">" + content + "</span>";
            }), new BBCode("color", function (content, attribs){
                // colour must be either a hex #xxx/#xxxxxx or a word with no spaces red/blue/ect.
                if(!isset(attribs["default"]) || !preg_match("/^(#[a-fA-F0-9]{3,6}|[A-Za-z]+)/", attribs["default"]))
                    attribs["default"] = "#000";

                return "<span style="color: " + attribs["default"] + "">" + content + "</span>";
            }),

            new BBCode("list", function (content, attribs){

                style = "circle";
                type = "ul";

                switch(attribs["default"]){
                    case "d":
                        style = "disc";
                        type = "ul";
                        break;
                    case "s":
                        style = "square";
                        type = "ul";
                        break;
                    case "1":
                        style = "decimal";
                        type = "ol";
                        break;
                    case "a":
                        style = "lower-alpha";
                        type = "ol";
                        break;
                    case "A":
                        style = "upper-alpha";
                        type = "ol";
                        break;
                    case "i":
                        style = "lower-roman";
                        type = "ol";
                        break;
                    case "I":
                        style = "upper-roman";
                        type = "ol";
                        break;
                }

                return "<{type} style=\"list-style: {style}\">{content}</{type}>";
            }, BBCode.BLOCK_TAG, false, [], ["*", "li", "ul", "li", "ol", "list"]), new BBCode("ul", "<ul>%content%</ul>", BBCode.BLOCK_TAG), new BBCode("ol", "<ol>%content%</ol>", BBCode.BLOCK_TAG), new BBCode("li", "<li>%content%</li>"), new BBCode("*", "<li>%content%</li>", BBCode.BLOCK_TAG, false, ["*", "li", "ul", "li", "ol", "/list"]),

            new BBCode("table", "<table>%content%</table>", BBCode.BLOCK_TAG, false, [], ["table", "th", "h", "tr", "row", "r", "td", "col", "c"]), new BBCode("th", "<th>%content%</th>"), new BBCode("h", "<th>%content%</th>"), new BBCode("tr", "<tr>%content%</tr>", BBCode.BLOCK_TAG, false, [], ["table", "th", "h", "tr", "row", "r", "td", "col", "c"]), new BBCode("row", "<tr>%content%</tr>", BBCode.BLOCK_TAG, false, [], ["table", "th", "h", "tr", "row", "r", "td", "col", "c"]), new BBCode("r", "<tr>%content%</tr>", BBCode.BLOCK_TAG, false, [], ["table", "th", "h", "tr", "row", "r", "td", "col", "c"]), new BBCode("td", "<td>%content%</td>"), new BBCode("col", "<td>%content%</td>"), new BBCode("c", "<td>%content%</td>"),

            new BBCode("notag", "%content%", BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL), new BBCode("nobbc", "%content%", BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL), new BBCode("noparse", "%content%", BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL),

            new BBCode("h1", "<h1>%content%</h1>"), new BBCode("h2", "<h2>%content%</h2>"), new BBCode("h3", "<h3>%content%</h3>"), new BBCode("h4", "<h4>%content%</h4>"), new BBCode("h5", "<h5>%content%</h5>"), new BBCode("h6", "<h6>%content%</h6>"), new BBCode("h7", "<h7>%content%</h7>"),

            new BBCode("big", "<span style="font-size: large">%content%</span>"), new BBCode("small", "<span style="font-size: x-small">%content%</span>"), // tables use this tag so can"t be a header.
            //new BBCode("h", "<h5>%content%</h5>"),

            new BBCode("br", "<br />", BBCode.INLINE_TAG, true), new BBCode("sp", "&nbsp;", BBCode.INLINE_TAG, true), new BBCode("hr", "<hr />", BBCode.INLINE_TAG, true),

            new BBCode("anchor", function (content, attribs, node){
                if(empty(attribs["default"])){
                    attribs["default"] = content;
                    // remove the content for [anchor]test[/anchor]
                    // usage as test is the anchor
                    content = "";
                }

                attribs["default"] = preg_replace("/[^a-zA-Z0-9_\-]+/", "", attribs["default"]);

                return "<a name=\"{attribs["default"]}\">{content}</a>";
            }), new BBCode("goto", function (content, attribs, node){
                if(empty(attribs["default"]))
                    attribs["default"] = content;

                attribs["default"] = preg_replace("/[^a-zA-Z0-9_\-#]+/", "", attribs["default"]);

                return "<a href=\"#{attribs["default"]}\">{content}</a>";
            }), new BBCode("jumpto", function (content, attribs, node){
                if(empty(attribs["default"]))
                    attribs["default"] = content;

                attribs["default"] = preg_replace("/[^a-zA-Z0-9_\-#]+/", "", attribs["default"]);

                return "<a href=\"#{attribs["default"]}\">{content}</a>";
            }),

            new BBCode("img", function (content, attribs, node){
                attrs = "";

                // for when default attrib is used for width x height
                if(isset(attribs["default"]) && preg_match("/[0-9]+[Xx\*][0-9]+/", attribs["default"])){
                    list(attribs["width"], attribs["height"]) = explode("x", attribs["default"]);
                    attribs["default"] = "";
                }// for when width & height are specified as the default attrib
                else if(isset(attribs["default"]) && is_numeric(attribs["default"])){
                    attribs["width"] = attribs["height"] = attribs["default"];
                    attribs["default"] = "";
                }

                // add alt tag if is one
                if(isset(attribs["default"]) && !empty(attribs["default"]))
                    attrs += " alt=\"{attribs["default"]}\"";else if(isset(attribs["alt"]))
                    attrs += " alt=\"{attribs["alt"]}\"";else
                    attrs += " alt=\"{content}\"";

                // width and height can only be numeric, anything else should be ignored to prevent XSS
                if(isset(attribs["width"]) && is_numeric(attribs["width"]))
                    attrs += " width=\"{attribs["width"]}\"";
                if(isset(attribs["height"]) && is_numeric(attribs["height"]))
                    attrs += " height=\"{attribs["height"]}\"";

                // add http:// to www starting urls
                if(strpos(content, "www") === 0)
                    content = "http://" + content;// add the base url to any urls not starting with http or ftp as they must be relative
                else if(substr(content, 0, 4) !== "http" && substr(content, 0, 3) !== "ftp")
                    content = node.root().get_base_uri() + content;

                return "<img{attrs} src=\"{content}\" />";
            }, BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL), new BBCode("email", function (content, attribs, node){
                if(empty(attribs["default"]))
                    attribs["default"] = content;

                return "<a href=\"mailto:{attribs["default"]}\">{content}</a>";
            }, BBCode.INLINE_TAG, false, [], ["text_node"], BBCode.AUTO_DETECT_EXCLUDE_ALL), new BBCode("url", function (content, attribs, node){
                if(empty(attribs["default"]))
                    attribs["default"] = content;

                // add http:// to www starting urls
                if(strpos(attribs["default"], "www") === 0)
                    attribs["default"] = "http://" + attribs["default"];// add the base url to any urls not starting with http or ftp as they must be relative
                else if(substr(attribs["default"], 0, 4) !== "http" && substr(attribs["default"], 0, 3) !== "ftp")
                    attribs["default"] = node.root().get_base_uri() + attribs["default"];

                return "<a href=\"{attribs["default"]}\">{content}</a>";
            }, BBCode.INLINE_TAG)];
    }

    /// <summery>
    /// Gets an array of the default Emoticons
    /// 
    /// <returns></returns> array
    /// </summery>
    public static function default_emoticons(){
        emoticons = [":)" => "https://" + DOMAIN + "/images/emoticons/smile.png", ":angel:" => "https://" + DOMAIN + "/images/emoticons/angel.png", ":angry:" => "https://" + DOMAIN + "/images/emoticons/angry.png", "8-)" => "https://" + DOMAIN + "/images/emoticons/cool.png", ":"(" => "https://" + DOMAIN + "/images/emoticons/cwy.png", ":ermm:" => "https://" + DOMAIN + "/images/emoticons/ermm.png", ":D" => "https://" + DOMAIN + "/images/emoticons/grin.png", "<3" => "https://" + DOMAIN + "/images/emoticons/heart.png", ":(" => "https://" + DOMAIN + "/images/emoticons/sad.png", ":O" => "https://" + DOMAIN + "/images/emoticons/shocked.png", ":P" => "https://" + DOMAIN + "/images/emoticons/tongue.png", ";)" => "https://" + DOMAIN + "/images/emoticons/wink.png", ":alien:" => "https://" + DOMAIN + "/images/emoticons/alien.png", ":blink:" => "https://" + DOMAIN + "/images/emoticons/blink.png", ":blush:" => "https://" + DOMAIN + "/images/emoticons/blush.png", ":cheerful:" => "https://" + DOMAIN + "/images/emoticons/cheerful.png", ":devil:" => "https://" + DOMAIN + "/images/emoticons/devil.png", ":dizzy:" => "https://" + DOMAIN + "/images/emoticons/dizzy.png", ":getlost:" => "https://" + DOMAIN + "/images/emoticons/getlost.png", ":happy:" => "https://" + DOMAIN + "/images/emoticons/happy.png", ":kissing:" => "https://" + DOMAIN + "/images/emoticons/kissing.png", ":ninja:" => "https://" + DOMAIN + "/images/emoticons/ninja.png", ":pinch:" => "https://" + DOMAIN + "/images/emoticons/pinch.png", ":pouty:" => "https://" + DOMAIN + "/images/emoticons/pouty.png", ":sick:" => "https://" + DOMAIN + "/images/emoticons/sick.png", ":sideways:" => "https://" + DOMAIN + "/images/emoticons/sideways.png", ":silly:" => "https://" + DOMAIN + "/images/emoticons/silly.png", ":sleeping:" => "https://" + DOMAIN + "/images/emoticons/sleeping.png", ":unsure:" => "https://" + DOMAIN + "/images/emoticons/unsure.png", ":woot:" => "https://" + DOMAIN + "/images/emoticons/w00t.png", ":wassat:" => "https://" + DOMAIN + "/images/emoticons/wassat.png", ":whistling:" => "https://" + DOMAIN + "/images/emoticons/whistling.png", ":love:" => "https://" + DOMAIN + "/images/emoticons/wub.png"];

        return emoticons;
    }

    /// <summery>
    /// Adds an emoticon to the parser.
    /// 
    /// <typeparam name=""></typeparam> string key
    /// <typeparam name=""></typeparam> string url
    /// <typeparam name=""></typeparam> bool   replace If to replace another emoticon with the same key
    /// <returns></returns> bool
    /// </summery>
    public function add_emoticon(key, url, replace = true){
        if(isset(this.emoticons[key]) && !replace)
            return false;

        this.emoticons[key] = url;
        return true;
    }

    /// <summery>
    /// Adds multipule emoticons to the parser. Should be in
    /// emoticon_key => image_url format.
    /// 
    /// <typeparam name=""></typeparam> Array emoticons
    /// <typeparam name=""></typeparam> bool  replace If to replace another emoticon with the same key
    /// </summery>
    public function add_emoticons(Array emoticons, replace = true){
        foreach(emoticons as key => url)
            this.add_emoticon(key, url, replace);
    }

    /// <summery>
    /// Removes an emoticon from the parser.
    /// 
    /// <typeparam name=""></typeparam> string key
    /// <returns></returns> bool
    /// </summery>
    public function remove_emoticon(key, url){
        if(!isset(this.emoticons[key]))
            return false;

        unset(this.emoticons[key]);
        return true;
    }

    /// <summery>
    /// Adds a bbcode to the parser
    /// 
    /// <typeparam name=""></typeparam> BBCode bbcode
    /// <typeparam name=""></typeparam> bool         replace If to replace another bbcode which is for the same tag
    /// <returns></returns> bool
    /// </summery>
    public function add_bbcode(BBCode bbcode, replace = true){
        if(!replace && isset(this.bbcodes[bbcode.tag()]))
            return false;

        this.bbcodes[bbcode.tag()] = bbcode;
        return true;
    }

    /// <summery>
    /// Adds an array of BBCode"s to the document
    /// 
    /// <typeparam name=""></typeparam> array bbcodes
    /// <typeparam name=""></typeparam> bool  replace
    /// <see cref="add_bbcode"/> 
    /// </summery>
    public function add_bbcodes(Array bbcodes, replace = true){
        foreach(bbcodes as bbcode)
            this.add_bbcode(bbcode, replace);
    }

    /// <summery>
    /// Removes a BBCode from the document
    /// 
    /// <typeparam name=""></typeparam> mixed bbcode String tag name or BBCode
    /// </summery>
    public function remove_bbcode(bbcode){
        if(bbcode instanceof BBCode)
            bbcode = bbcode.tag();

        unset(this.bbcodes[bbcode]);
    }

    /// <summery>
    /// Gets an array of bbcode tags that will currently be processed
    /// 
    /// <returns></returns> array
    /// </summery>
    public function list_bbcodes(){
        return array_keys(this.bbcodes);
    }

    /// <summery>
    /// Returns the BBCode object for the passed
    /// tag.
    /// 
    /// <typeparam name=""></typeparam> string tag
    /// <returns></returns> BBCode
    /// </summery>
    public function get_bbcode(tag){
        if(!isset(this.bbcodes[tag]))
            return null;

        return this.bbcodes[tag];
    }

    /// <summery>
    /// Gets the base URI to be used in links, images, ect.
    /// 
    /// <returns></returns> string
    /// </summery>
    public function get_base_uri(){
        if(this.base_uri != null)
            return this.base_uri;
        return "";
        //        return htmlentities(dirname(_SERVER["PHP_SELF"]), ENT_QUOTES | ENT_IGNORE, "UTF-8") + "/";
    }

    /// <summery>
    /// Sets the base URI to be used in links, images, ect.
    /// 
    /// <typeparam name=""></typeparam> string uri
    /// </summery>
    public function set_base_uri(uri){
        this.base_uri = uri;
    }

    /// <summery>
    /// Parses a BBCode string into the current document
    /// 
    /// <typeparam name=""></typeparam> string str
    /// <returns></returns> BBCodeNodeContainerDocument
    /// @throws BBCodeInvalidNestingException
    /// @throws Exception
    /// </summery>
    public function parse(str){
        /*        str      = preg_replace("/[\r\n|\r]/", "\n", str);*/

        len = strlen(str);
        tag_open = false;
        tag_text = "";
        tag = "";

        // set the document as the current tag.
        this.current_tag = this;

        for(i = 0; i < len; ++i){
            if(str[i] === "["){
                if(tag_open)
                    tag_text += "[" + tag;

                tag_open = true;
                tag = "";
            }else if(str[i] === "]" && tag_open){
                if(tag !== ""){
                    bits = preg_split("/([ =])/", trim(tag), 2, PREG_SPLIT_DELIM_CAPTURE);

                    tag_attrs = (isset(bits[2]) ? bits[1] + bits[2] : "");
                    tag_closing = (bits[0][0] === "/");
                    tag_name = (bits[0][0] === "/" ? substr(bits[0], 1) : bits[0]);

                    if(isset(this.bbcodes[tag_name])){
                        this.tag_text(tag_text);
                        tag_text = "";

                        if(tag_closing){
                            if(!this.tag_close(tag_name))
                                tag_text = "[{tag}]";
                        }else{
                            if(!this.tag_open(tag_name, this.parse_attribs(tag_attrs)))
                                tag_text = "[{tag}]";
                        }
                    }else
                        tag_text += "[{tag}]";
                }else
                    tag_text += "[]";

                tag_open = false;
                tag = "";
            }else if(tag_open)
                tag += str[i];else
                tag_text += str[i];
        }

        this.tag_text(tag_text);

        if(this.throw_errors && !this.current_tag instanceof BBCodeNodeContainerDocument)
            throw new Exception("Missing closing tag for tag [{this.current_tag.tag()}]");

        return this;
    }

    /// <summery>
    /// Handles a BBCode opening tag
    /// 
    /// <typeparam name=""></typeparam> string tag
    /// <typeparam name=""></typeparam> array  attrs
    /// <returns></returns> bool
    /// @throws BBCodeInvalidNestingException
    /// </summery>
    private function tag_open(tag, attrs){
        if(this.current_tag instanceof BBCodeNodeContainerTag){
            closing_tags = this.bbcodes[this.current_tag.tag()].closing_tags();

            if(in_array(tag, closing_tags))
                this.tag_close(this.current_tag.tag());
        }

        if(this.current_tag instanceof BBCodeNodeContainerTag){
            accepted_children = this.bbcodes[this.current_tag.tag()].accepted_children();

            if(!empty(accepted_children) && !in_array(tag, accepted_children))
                return false;

            if(this.throw_errors && !this.bbcodes[tag].is_inline() && this.bbcodes[this.current_tag.tag()].is_inline())
                throw new BBCodeInvalidNestingException("Block level tag [{tag}] was opened within an inline tag [{this.current_tag.tag()}]");
        }

        node = new BBCodeNodeContainerTag(tag, attrs);
        this.current_tag.add_child(node);

        if(!this.bbcodes[tag].is_self_closing())
            this.current_tag = node;

        return true;
    }

    /// <summery>
    /// Handles tag text
    /// 
    /// <typeparam name=""></typeparam> string text
    /// <returns></returns> void
    /// </summery>
    private function tag_text(text){
        if(this.current_tag instanceof BBCodeNodeContainerTag){
            accepted_children = this.bbcodes[this.current_tag.tag()].accepted_children();

            if(!empty(accepted_children) && !in_array("text_node", accepted_children))
                return;
        }

        this.current_tag.add_child(new BBCodeNodeText(text));
    }

    /// <summery>
    /// Handles BBCode closing tag
    /// 
    /// <typeparam name=""></typeparam> string tag
    /// <returns></returns> bool
    /// </summery>
    private function tag_close(tag){
        if(!this.current_tag instanceof BBCodeNodeContainerDocument && tag !== this.current_tag.tag()){
            closing_tags = this.bbcodes[this.current_tag.tag()].closing_tags();

            if(in_array(tag, closing_tags) || in_array("/" + tag, closing_tags))
                this.current_tag = this.current_tag.parent();
        }

        if(this.current_tag instanceof BBCodeNodeContainerDocument)
            return false;else if(tag !== this.current_tag.tag()){
            // check if this is a tag inside another tag like
            // [tag1] [tag2] [/tag1] [/tag2]
            node = this.current_tag.find_parent_by_tag(tag);

            if(node !== null){
                this.current_tag = node.parent();

                while((node = node.last_tag_node()) !== null){
                    new_node = new BBCodeNodeContainerTag(node.tag(), node.attributes());
                    this.current_tag.add_child(new_node);
                    this.current_tag = new_node;
                }
            }else
                return false;
        }else
            this.current_tag = this.current_tag.parent();

        return true;
    }

    /// <summery>
    /// Parses a bbcode attribute string into an array
    /// 
    /// <typeparam name=""></typeparam> string attribs
    /// <returns></returns> array
    /// </summery>
    private function parse_attribs(attribs){
        ret = ["default" => null];
        attribs = trim(attribs);

        if(attribs == "")
            return ret;

        // if this tag only has one = then there is only one attribute
        // so add it all to default
        if(attribs[0] == "=" && strrpos(attribs, "=") === 0)
            ret["default"] = htmlentities(substr(attribs, 1), ENT_QUOTES | ENT_IGNORE, "UTF-8");else{
            preg_match_all("/(\S+)=((?:(?:(["\"])(?:\\\3|[^\3])*?\3))|(?:[^\""\s]+))/", attribs, matches, PREG_SET_ORDER);

            foreach(matches as match)
                ret[match[1]] = htmlentities(match[2], ENT_QUOTES | ENT_IGNORE, "UTF-8");
        }

        return ret;
    }

    private function loop_text_nodes(func, array exclude = [], BBCodeNodeContainer node = null){
        if(node === null)
            node = this;

        foreach(node.children() as child){
            if(child instanceof BBCodeNodeContainerTag){
                if(!in_array(child.tag(), exclude))
                    this.loop_text_nodes(func, exclude, child);
            }else if(child instanceof BBCodeNodeText){
                func(child);
            }
        }
    }

    /// <summery>
    /// Detects any none clickable links and makes them clickable
    /// 
    /// <returns></returns> BBCodeNodeContainerDocument
    /// </summery>
    public function detect_links(){
        this.loop_text_nodes(function (child){
            preg_match_all("/(?:(?:https?|ftp):\/\/|(?:www|ftp)\.)(?:[a-zA-Z0-9\-\.]{1,255}\.[a-zA-Z]{1,20})(?.[0-9]{1,5})?(?:\/[^\s"\"]*)?(?:(?<![,\)\.])|[\S])/", child.get_text(), matches, PREG_PATTERN_ORDER | PREG_OFFSET_CAPTURE);

            if(count(matches[0]) == 0)
                return;

            replacment = [];
            last_pos = 0;

            foreach(matches[0] as match){
                if(substr(match[0], 0, 3) === "ftp" && match[0][3] !== ":")
                    url = "ftp://" + match[0];else if(match[0][0] === "w")
                    url = "http://" + match[0];else
                    url = match[0];

                url = new BBCodeNodeContainerTag("url", ["default" => htmlentities(url, ENT_QUOTES | ENT_IGNORE, "UTF-8")]);
                url_text = new BBCodeNodeText(match[0]);
                url.add_child(url_text);

                replacment[] = new BBCodeNodeText(substr(child.get_text(), last_pos, match[1] - last_pos));
                replacment[] = url;
                last_pos = match[1] + strlen(match[0]);
            }

            replacment[] = new BBCodeNodeText(substr(child.get_text(), last_pos));
            child.parent().replace_child(child, replacment);
        }, this.get_excluded_tags(BBCode.AUTO_DETECT_EXCLUDE_URL));

        return this;
    }

    /// <summery>
    /// Detects any none clickable emails and makes them clickable
    /// 
    /// <returns></returns> BBCodeNodeContainerDocument
    /// </summery>
    public function detect_emails(){
        this.loop_text_nodes(function (child){
            preg_match_all("/(?:[a-zA-Z0-9\-\._]){1,}@(?:[a-zA-Z0-9\-\.]{1,255}\.[a-zA-Z]{1,20})/", child.get_text(), matches, PREG_PATTERN_ORDER | PREG_OFFSET_CAPTURE);

            if(count(matches[0]) == 0)
                return;

            replacment = [];
            last_pos = 0;

            foreach(matches[0] as match){
                url = new BBCodeNodeContainerTag("email", []);
                url_text = new BBCodeNodeText(match[0]);
                url.add_child(url_text);

                replacment[] = new BBCodeNodeText(substr(child.get_text(), last_pos, match[1] - last_pos));
                replacment[] = url;
                last_pos = match[1] + strlen(match[0]);
            }

            replacment[] = new BBCodeNodeText(substr(child.get_text(), last_pos));
            child.parent().replace_child(child, replacment);
        }, this.get_excluded_tags(BBCode.AUTO_DETECT_EXCLUDE_EMAIL));

        return this;
    }

    /// <summery>
    /// Detects any emoticons and replaces them with their images
    /// 
    /// <returns></returns> BBCodeNodeContainerDocument
    /// </summery>
    public function detect_emoticons(){
        pattern = "";
        foreach(this.emoticons as key => url)
            pattern += (pattern === "" ? "/(?:" : "|") + preg_quote(key, "/");
        pattern += ")/";

        emoticons = this.emoticons;

        this.loop_text_nodes(function (child) use (pattern, emoticons){
            preg_match_all(pattern, child.get_text(), matches, PREG_PATTERN_ORDER | PREG_OFFSET_CAPTURE);

            if(count(matches[0]) == 0)
                return;

            replacment = [];
            last_pos = 0;

            foreach(matches[0] as match){
                url = new BBCodeNodeContainerTag("img", ["alt" => match[0]]);
                url_text = new BBCodeNodeText(emoticons[match[0]]);
                url.add_child(url_text);

                replacment[] = new BBCodeNodeText(substr(child.get_text(), last_pos, match[1] - last_pos));
                replacment[] = url;
                last_pos = match[1] + strlen(match[0]);
            }

            replacment[] = new BBCodeNodeText(substr(child.get_text(), last_pos));
            child.parent().replace_child(child, replacment);
        }, this.get_excluded_tags(BBCode.AUTO_DETECT_EXCLUDE_EMOTICON));

        return this;
    }

    /// <summery>
    /// Gets an array of tages to be excluded from
    /// the elcude param
    /// 
    /// <typeparam name=""></typeparam> int exclude What to gets excluded from i.e. BBCode.AUTO_DETECT_EXCLUDE_EMOTICON
    /// <returns></returns> array
    /// </summery>
    private function get_excluded_tags(exclude){
        ret = [];

        foreach(this.bbcodes as bbcode)
            if(bbcode.auto_detect_exclude() & exclude)
                ret[] = bbcode.tag();

        return ret;
    }
}
