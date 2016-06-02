tinyMCE.init({
    theme: "advanced",
    schema: "html5",
    mode: "specific_textareas",
    editor_selector: "tinymce",
    plugins: "paste,fullscreen,autoresize,searchreplace,inlinepopups,template,table,media",//, medialibrary
    theme_advanced_toolbar_location: "top",
    theme_advanced_toolbar_align: "left",
    theme_advanced_buttons1: "search,replace,|,cut,copy,paste,|,undo,redo,media,|,link,unlink,charmap,|,justifyleft,justifycenter,justifyright,justifyfull,|,bold,italic,|,numlist,bullist,formatselect,|,code,fullscreen,",//,medialibrary
    theme_advanced_buttons2: "anchor,cleanup,|,outdent,indent,|,forecolor,backcolor,|,formatselect,fontselect,fontsizeselect,|,tablecontrols,autosave",
    theme_advanced_buttons3: "",
    convert_urls: false,
    //clear copy in word
    paste_auto_cleanup_on_paste: true,
    paste_preprocess: function (pl, o) {
        // Content string containing the HTML from the clipboard
        //alert(o.content);
        o.content = o.content;
    },
    paste_postprocess: function (pl, o) {
        // Content DOM node containing the DOM structure of the clipboard
        //alert(o.node.innerHTML);
        o.node.innerHTML = o.node.innerHTML;
    },
    apply_source_formatting: true,
    paste_remove_styles: true,
    paste_remove_styles_if_webkit: true,
    paste_strip_class_attributes: true,
    paste_auto_cleanup_on_paste: true,
    paste_remove_spans: true,
    paste_remove_styles: true,
    paste_retain_style_properties: "",
    valid_elements: "*[*]",
    // shouldn't be needed due to the valid_elements setting, but TinyMCE would strip script.src without it.
    extended_valid_elements: "script[type|defer|src|language]"
});