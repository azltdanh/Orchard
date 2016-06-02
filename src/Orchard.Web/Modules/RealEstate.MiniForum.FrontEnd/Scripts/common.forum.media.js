tinyMCE.init({
    theme: "advanced",
    schema: "html5",
    mode: "specific_textareas",
    editor_selector: "tinymce",
    plugins: "paste,fullscreen,autoresize,searchreplace,inlinepopups,template,mediapicker",
    theme_advanced_toolbar_location: "top",
    theme_advanced_toolbar_align: "left",
    theme_advanced_buttons1: "search,replace,|,cut,copy,paste,|,undo,redo, mediapicker,|,justifyleft,justifycenter,justifyright,justifyfull,|,link,unlink,charmap,|,bold,italic,|,numlist,bullist,formatselect,|,code,fullscreen,",
    theme_advanced_buttons2: "anchor,cleanup,|,outdent,indent,|,forecolor,backcolor,|,formatselect,fontselect,fontsizeselect",
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

// Post - Comment
$("#areatinymceContent").focus(function () {
    $(this).fadeOut("1000");
    $(this).prev().fadeIn("1000");
    tinyMCE.activeEditor.focus();
});

jQuery(function ($) {
    //Character Limit Counter
    (function ($) { $.fn.extend({ limiter: function (limit, elem) { $(this).on("keyup focus", function () { setCount(this, elem); }); function setCount(src, elem) { var chars = src.value.length; if (chars > limit) { src.value = src.value.substr(0, limit); chars = limit; } elem.html(limit - chars); } setCount($(this)[0], elem); } }); })(jQuery);
    $("form").bind("orchard-admin-pickimage-open", function (ev, data) {
        data = data || {};
        UploadImgMedia();
    });
    // Giới hạn số lượng ký tự nhập vào
    if ($("#charsTitle").length != 0) { var elemTitle = $("#charsTitle"); $("#Title").limiter(100, elemTitle); }
    // Hide button default
    $(".edit-item-secondary").hide();
});
function UploadImgMedia() {
    $('#UploadMediaModalId').modal('show'); $('#imginsert').click(); $(".field_imgupload").show(); $(".field_filemanagement").hide();
}
function FileAttachmentMedia() {
    $('#UploadMediaModalId').modal('show'); $("#fileinsert").click(); $(".field_imgupload").hide(); $(".field_filemanagement").show();
}
//
function GetNameForAlt() {
    var alt = "định giá nhà đất";
    if ($("#Name").length > 0) {
        if ($("#Name").val() == "") {
            return alt;
        } else {
            return alt = $("#Name").val();
        }
    } else {
        return alt;
    }
}

function AddImgUrl(value) {
    var curent = $(value).parent().prev().find("input");
    if ($(curent).val() == "" || $(curent).val() == null) {
        alert("Vui lòng chèn link ảnh vào!");
        $(curent).focus();
        return false;
    }
    var ch = $(curent).val();
    var ck = ($(curent).val()).split('.');
    if (ck[ck.length - 1] == "jpg" || ck[ck.length - 1] == "png" || ck[ck.length - 1] == "gif" || ck[ck.length - 1] == "bmp" || ck[ck.length - 1] == "JPG" || ck[ck.length - 1] == "JPEG" || ck[ck.length - 1] == "PNG" || ck[ck.length - 1] == "GIF" || ck[ck.length - 1] == "BMP") {
        var cu = $("#count_img").val();
        var img = '<img src="' + $(curent).val() + '?width=430" alt="' + GetNameForAlt() + '"/>';

        alert("Thêm thành công!");
        tinyMCE.activeEditor.execCommand('mceInsertContent', false, img);
        $(curent).focus();
    } else {
        alert("Chỉ được upload ảnh có định dạng: jpg, jpeg, png, gif, bmp. Bạn vui lòng chọn ảnh khác!!");
        $(curent).focus();
        return false;
    }
}
//uploadify
$("#file_upload_post").uploadify({
    swf: '/Modules/RealEstate.Admin/Scripts/uploadify-v3.1/uploadify.swf',
    uploader: '/ajax/upload/image-post',
    cancelImg: '/Modules/RealEstate.Admin/Scripts/uploadify-v3.1/uploadify-cancel.png',
    buttonText: 'Chọn hình ảnh',
    buttonClass: 'button btn-success',
    width: 90,
    multi: false,
    height: 25,
    fileObjName: 'file',
    fileSizeLimit: '10MB',
    fileTypeDesc: 'Image Files',
    fileTypeExts: '*.jpg;*.jpeg;*.gif;*.png',
    formData: { __RequestVerificationToken: antiForgeryToken },//, folderName: $("#foldernameUpload").val()
    onUploadSuccess: function (file, data, response) {
        var json = jQuery.parseJSON(data);
        var img = '';
        if (json._w < 530) {
            img = '<img src="' + json.path + '?width=' + json._w + '" alt="' + GetNameForAlt() + '"/>';
        } else {
            img = '<img src="' + json.path + '?width=530" alt="' + GetNameForAlt() + '"/>';
        }
        tinyMCE.activeEditor.execCommand('mceInsertContent', false, img);
        alert("Thêm thành công!");
    },
    debug: false
});

//08.02.2013 - File management
$("#file_upload_management").uploadify({
    swf: '/Modules/RealEstate.Admin/Scripts/uploadify-v3.1/uploadify.swf',
    uploader: '/ajax/upload/file-attachment',
    cancelImg: '/Modules/RealEstate.Admin/Scripts/uploadify-v3.1/uploadify-cancel.png',
    buttonText: 'Chọn tập tin',
    buttonClass: 'button btn-success',
    width: 85,
    multi: false,
    height: 25,
    fileObjName: 'file',
    fileSizeLimit: '5MB',
    fileTypeDesc: 'Attach Files',
    fileTypeExts: '*.jpg;*.jpeg;*.gif;*.png; *.doc; *.docx; *.xlsx; *.xls; *.ppt; *.pdf',
    formData: { __RequestVerificationToken: antiForgeryToken, folderNames: $("#foldernameUpload").val() },
    onUploadSuccess: function (file, data, response) {
        alert("upload tập tin thành công!");
        var json = jQuery.parseJSON(data);
        var file = '<a href="' + json.path + '" title="tập tin đính kèm">' + json.name + '</a>';
        tinyMCE.activeEditor.execCommand('mceInsertContent', false, file);
    },
    debug: false
});
$("#imginsert").click(function () {
    $(".field_imgupload").show();
    $(".field_filemanagement").hide();
});
$("#fileinsert").click(function () {
    $(".field_imgupload").hide();
    $(".field_filemanagement").show();
});
/**Avatar upload */
$("#Profile_Avatars_FileUpload").uploadify({
    swf: '/Modules/RealEstate.Admin/Scripts/uploadify-v3.1/uploadify.swf',
    uploader: '/ajax/userprofile/avatarupdate',
    cancelImg: '/Modules/RealEstate.Admin/Scripts/uploadify-v3.1/uploadify-cancel.png',
    buttonText: 'Chọn hình',
    buttonClass: 'button btn-success',
    width: 85,
    multi: false,
    height: 21,
    fileObjName: 'file',
    fileSizeLimit: '10MB',
    fileTypeDesc: 'Image Files',
    fileTypeExts: '*.jpg;*.jpeg;*.gif;*.png',
    formData: { __RequestVerificationToken: antiForgeryToken },
    onUploadSuccess: function (file, data, response) {
        var json = jQuery.parseJSON(data);
        if (json.success) {
            alert('Avatar đã được cập nhật. Bạn có thể load (Ctrl + F5) lại trang để cập nhật sự thay đổi.');
        } else {
            alert('Lỗi upload file. FileName: ' + json.name);
        }
    },
    debug: false
});