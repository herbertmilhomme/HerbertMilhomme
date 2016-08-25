using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for BBCodeNode
/// </summary>

abstract class BBCodeNode {

    /// <summery>
    /// Nodes parent
    /// 
    /// @var BBCodeNodeContainer
    /// </summery>
    protected parent;

    /// <summery>
    /// Nodes root parent
    /// 
    /// @var BBCodeNodeContainer
    /// </summery>
    protected root;

    /// <summery>
    /// Sets the nodes parent
    /// 
    /// <typeparam name=""></typeparam> BBCodeNode|BBCodeNodeContainer parent
    /// </summery>
    public function set_parent(BBCodeNodeContainer parent = null){
        this.parent = parent;

        if(parent instanceof BBCodeNodeContainerDocument)
            this.root = parent;else
            this.root = parent.root();
    }

    /// <summery>
    /// Gets the nodes parent. Returns null if there
    /// is no parent
    /// 
    /// <returns></returns> BBCodeNode
    /// </summery>
    public function parent(){
        return this.parent;
    }

    /// <summery>
    /// <returns></returns> string
    /// </summery>
    public function get_html(){
        return null;
    }

    /// <summery>
    /// Gets the nodes root node
    /// 
    /// <returns></returns> BBCodeNode
    /// </summery>
    public function root(){
        return this.root;
    }

    /// <summery>
    /// Finds a parent node of the passed type.
    /// Returns null if none found.
    /// 
    /// <typeparam name=""></typeparam> string tag
    /// <returns></returns> BBCodeNodeContainerTag
    /// </summery>
    public function find_parent_by_tag(tag){
        node = this.parent();

        while(this.parent() != null && !node instanceof BBCodeNodeContainerDocument){
            if(node.tag() === tag)
                return node;

            node = node.parent();
        }

        return null;
    }
}