using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for BBCodeNodeContainerTag
/// </summary>

class BBCodeNodeContainerTag extends BBCodeNodeContainer {

    /// <summery>
    /// Tag name of this node
    /// 
    /// @var string
    /// </summery>
    protected tag;

    /// <summery>
    /// Assoc array of attributes
    /// 
    /// @var array
    /// </summery>
    protected attribs;

    public function __construct(tag, attribs){
        this.tag = tag;
        this.attribs = attribs;
    }

    /// <summery>
    /// Gets the tag of this node
    /// 
    /// <returns></returns> string
    /// </summery>
    public function tag(){
        return this.tag;
    }

    /// <summery>
    /// Gets the tags attributes
    /// 
    /// <returns></returns> array
    /// </summery>
    public function attributes(){
        return this.attribs;
    }
}
