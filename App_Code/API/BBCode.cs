using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for BBCode
/// </summary>

/// <summery>
/// BBCode
/// </summery>
class BBCode {

    /// <summery>
    /// The tag this BBCode applies to
    /// 
    /// @var string
    /// </summery>
    protected tag;

    /// <summery>
    /// The BBCodes handler
    /// 
    /// @var mixed string or function
    /// </summery>
    protected handler;

    /// <summery>
    /// If the tag is a self closing tag
    /// 
    /// @var bool
    /// </summery>
    protected is_self_closing;

    /// <summery>
    /// Array of tags which will cause this tag to close
    /// if they are encountered before the end of it.
    /// Used for [*] which may not have a closing tag so
    /// other [*] or [/list] tags will cause it to be closed�
    /// 
    /// @var array
    /// </summery>
    protected closing_tags;

    /// <summery>
    /// Valid child nodes for this tag. Tags like list, table,
    /// ect. will only accept li, tr, ect. tags and not text nodes
    /// 
    /// @var array
    /// </summery>
    protected accepted_children;

    /// <summery>
    /// Which auto detections this BBCode should be excluded from
    /// 
    /// @var int
    /// </summery>
    protected is_inline;

    const AUTO_DETECT_EXCLUDE_NONE = 0;
    const AUTO_DETECT_EXCLUDE_URL = 2;
    const AUTO_DETECT_EXCLUDE_EMAIL = 4;
    const AUTO_DETECT_EXCLUDE_EMOTICON = 8;
    const AUTO_DETECT_EXCLUDE_ALL = 15;

    const BLOCK_TAG = false;
    const INLINE_TAG = true;

    /// <summery>
    /// Creates a new BBCode
    /// 
    /// <typeparam name=""></typeparam> string     tag                 Tag this BBCode is for
    /// <typeparam name=""></typeparam> mixed      handler             String or function, should return a string
    /// <typeparam name=""></typeparam> bool       is_inline           If this tag is an inline tag or a block tag
    /// <typeparam name=""></typeparam> array|bool is_self_closing     If this tag is self closing, I.E. doesn"t need [/tag]
    /// <typeparam name=""></typeparam> array      closing_tags        Tags which will close this tag
    /// <typeparam name=""></typeparam> array|int  accepted_children   Tags allowed as children of this BBCode. Can also include text_node
    /// <typeparam name=""></typeparam> int        auto_detect_exclude Which auto detections to exclude this BBCode from
    /// </summery>
    public function __construct(tag, handler, is_inline = BBCode.INLINE_TAG, is_self_closing = false, closing_tags = [], accepted_children = [], auto_detect_exclude = BBCode.AUTO_DETECT_EXCLUDE_NONE){
        this.tag = tag;
        this.is_inline = is_inline;
        this.handler = handler;
        this.is_self_closing = is_self_closing;
        this.closing_tags = closing_tags;
        this.accepted_children = accepted_children;
        this.auto_detect_exclude = auto_detect_exclude;
    }

    /// <summery>
    /// Gets the tag name this BBCode is for
    /// 
    /// <returns></returns> string
    /// </summery>
    public function tag(){
        return this.tag;
    }

    /// <summery>
    /// Gets if this BBCode is inline or if it"s block
    /// 
    /// <returns></returns> bool
    /// </summery>
    public function is_inline(){
        return this.is_inline;
    }

    /// <summery>
    /// Gets if this BBCode is self closing
    /// 
    /// <returns></returns> bool
    /// </summery>
    public function is_self_closing(){
        return this.is_self_closing;
    }

    /// <summery>
    /// Gets the format string/handler for this BBCode
    /// 
    /// <returns></returns> mixed String or function
    /// </summery>
    public function handler(){
        return this.handler;
    }

    /// <summery>
    /// Gets an array of tags which will cause this tag to be closed
    /// 
    /// <returns></returns> array
    /// </summery>
    public function closing_tags(){
        return this.closing_tags;
    }

    /// <summery>
    /// Gets an array of tags which are allowed as children of this tag
    /// 
    /// <returns></returns> array
    /// </summery>
    public function accepted_children(){
        return this.accepted_children;
    }

    /// <summery>
    ///  Which auto detections this BBCode should be excluded from
    /// 
    /// <returns></returns> int
    /// </summery>
    public function auto_detect_exclude(){
        return this.auto_detect_exclude;
    }
}