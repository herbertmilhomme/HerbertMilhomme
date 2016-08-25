using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for BBCodeNodeContainer
/// </summary>

abstract class BBCodeNodeContainer extends BBCodeNode {

    /// <summery>
    /// Array of child nodes
    /// 
    /// @var array
    /// </summery>
    protected children = [];

    /// <summery>
    /// Adds a BBCodeNode as a child
    /// of this node.
    /// 
    /// <typeparam name=""></typeparam> BBCodeNode|The child The child node to add
    /// </summery>
    public function add_child(BBCodeNode child){
        this.children[] = child;
        child.set_parent(this);
    }

    /// <summery>
    /// Replaces a child node
    /// 
    /// <typeparam name=""></typeparam> BBCodeNode what
    /// <typeparam name=""></typeparam> mixed            with BBCodeNode or an array of BBCodeNode
    /// <returns></returns> bool
    /// </summery>
    public function replace_child(BBCodeNode what, with){
        replace_key = array_search(what, this.children);

        if(replace_key === false)
            return false;

        if(is_array(with))
            foreach(with as child)
                child.set_parent(this);

        array_splice(this.children, replace_key, 1, with);

        return true;
    }

    /// <summery>
    /// Removes a child fromthe node
    /// 
    /// <typeparam name=""></typeparam> BBCodeNode child
    /// <returns></returns> bool
    /// </summery>
    public function remove_child(BBCodeNode child){
        key = array_search(what, this.children);

        if(key === false)
            return false;

        this.children[key].set_parent();
        unset(this.children[key]);
        return true;
    }

    /// <summery>
    /// Gets the nodes children
    /// 
    /// <returns></returns> array
    /// </summery>
    public function children(){
        return this.children;
    }

    /// <summery>
    /// Gets the last child of type BBCodeNodeContainerTag.
    /// 
    /// <returns></returns> BBCodeNodeContainerTag
    /// </summery>
    public function last_tag_node(){
        children_len = count(this.children);

        for(i = children_len - 1; i >= 0; i--)
            if(this.children[i] instanceof BBCodeNodeContainerTag)
                return this.children[i];

        return null;
    }

    /// <summery>
    /// Gets a HTML representation of this node
    /// 
    /// <returns></returns> string
    /// </summery>
    public function get_html(nl2br = true){
        html = "";

        foreach(this.children as child){
            if(isset(child.tag) && child.tag == "code"){
                //                str = html_entity_decode(child.get_html(nl2br));
                str = child.get_text(nl2br);

                str = str_replace(["<", ">"], ["&lt;", "&gt;"], str);
                //                str = str_replace(array("&lt;pre&gt;&lt;code&gt;", "&lt;/code&gt;&lt;/pre&gt;", "&lt;br&gt;"), array("<pre><code>", "</code></pre>", "<br>"), str);
                str = str_replace(["\r\n", "\n\r", "\n", "\r"], ["<br>", "<br>", "<br>", "<br>"], str);
                html += "<pre><code>" + str + "</code></pre>";

            }else{
                html += child.get_html(nl2br);
            }

        }

        if(this instanceof BBCodeNodeContainerDocument)
            return html;

        bbcode = this.root().get_bbcode(this.tag);

        if(is_callable(bbcode.handler()) && (func = bbcode.handler()) !== false)
            return func(html, this.attribs, this);
        //return call_user_func(bbcode.handler(), html, this.attribs, this);

        return str_replace("%content%", html, bbcode.handler());
    }

    /// <summery>
    /// Gets the raw text content of this node
    /// and it"s children.
    /// The returned text is UNSAFE and should not
    /// be used without filtering!
    /// 
    /// <returns></returns> string
    /// </summery>
    public function get_text(){
        text = "";

        foreach(this.children as child)
            text += child.get_text();

        return text;
    }
}