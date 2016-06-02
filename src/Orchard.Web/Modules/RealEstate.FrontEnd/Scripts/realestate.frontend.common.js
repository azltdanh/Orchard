// jquery.tipTip.minified.js
(function ($) { $.fn.tipTip = function (options) { var defaults = { activation: "hover", keepAlive: false, maxWidth: "200px", edgeOffset: 3, defaultPosition: "bottom", delay: 400, fadeIn: 200, fadeOut: 200, attribute: "title", content: false, enter: function () { }, exit: function () { } }; var opts = $.extend(defaults, options); if ($("#tiptip_holder").length <= 0) { var tiptip_holder = $('<div id="tiptip_holder" style="max-width:' + opts.maxWidth + ';"></div>'); var tiptip_content = $('<div id="tiptip_content"></div>'); var tiptip_arrow = $('<div id="tiptip_arrow"></div>'); $("body").append(tiptip_holder.html(tiptip_content).prepend(tiptip_arrow.html('<div id="tiptip_arrow_inner"></div>'))) } else { var tiptip_holder = $("#tiptip_holder"); var tiptip_content = $("#tiptip_content"); var tiptip_arrow = $("#tiptip_arrow") } return this.each(function () { var org_elem = $(this); if (opts.content) { var org_title = opts.content } else { var org_title = org_elem.attr(opts.attribute) } if (org_title != "") { if (!opts.content) { org_elem.removeAttr(opts.attribute) } var timeout = false; if (opts.activation == "hover") { org_elem.hover(function () { active_tiptip() }, function () { if (!opts.keepAlive) { deactive_tiptip() } }); if (opts.keepAlive) { tiptip_holder.hover(function () { }, function () { deactive_tiptip() }) } } else if (opts.activation == "focus") { org_elem.focus(function () { active_tiptip() }).blur(function () { deactive_tiptip() }) } else if (opts.activation == "click") { org_elem.click(function () { active_tiptip(); return false }).hover(function () { }, function () { if (!opts.keepAlive) { deactive_tiptip() } }); if (opts.keepAlive) { tiptip_holder.hover(function () { }, function () { deactive_tiptip() }) } } function active_tiptip() { opts.enter.call(this); tiptip_content.html(org_title); tiptip_holder.hide().removeAttr("class").css("margin", "0"); tiptip_arrow.removeAttr("style"); var top = parseInt(org_elem.offset()['top']); var left = parseInt(org_elem.offset()['left']); var org_width = parseInt(org_elem.outerWidth()); var org_height = parseInt(org_elem.outerHeight()); var tip_w = tiptip_holder.outerWidth(); var tip_h = tiptip_holder.outerHeight(); var w_compare = Math.round((org_width - tip_w) / 2); var h_compare = Math.round((org_height - tip_h) / 2); var marg_left = Math.round(left + w_compare); var marg_top = Math.round(top + org_height + opts.edgeOffset); var t_class = ""; var arrow_top = ""; var arrow_left = Math.round(tip_w - 12) / 2; if (opts.defaultPosition == "bottom") { t_class = "_bottom" } else if (opts.defaultPosition == "top") { t_class = "_top" } else if (opts.defaultPosition == "left") { t_class = "_left" } else if (opts.defaultPosition == "right") { t_class = "_right" } var right_compare = (w_compare + left) < parseInt($(window).scrollLeft()); var left_compare = (tip_w + left) > parseInt($(window).width()); if ((right_compare && w_compare < 0) || (t_class == "_right" && !left_compare) || (t_class == "_left" && left < (tip_w + opts.edgeOffset + 5))) { t_class = "_right"; arrow_top = Math.round(tip_h - 13) / 2; arrow_left = -12; marg_left = Math.round(left + org_width + opts.edgeOffset); marg_top = Math.round(top + h_compare) } else if ((left_compare && w_compare < 0) || (t_class == "_left" && !right_compare)) { t_class = "_left"; arrow_top = Math.round(tip_h - 13) / 2; arrow_left = Math.round(tip_w); marg_left = Math.round(left - (tip_w + opts.edgeOffset + 5)); marg_top = Math.round(top + h_compare) } var top_compare = (top + org_height + opts.edgeOffset + tip_h + 8) > parseInt($(window).height() + $(window).scrollTop()); var bottom_compare = ((top + org_height) - (opts.edgeOffset + tip_h + 8)) < 0; if (top_compare || (t_class == "_bottom" && top_compare) || (t_class == "_top" && !bottom_compare)) { if (t_class == "_top" || t_class == "_bottom") { t_class = "_top" } else { t_class = t_class + "_top" } arrow_top = tip_h; marg_top = Math.round(top - (tip_h + 5 + opts.edgeOffset)) } else if (bottom_compare | (t_class == "_top" && bottom_compare) || (t_class == "_bottom" && !top_compare)) { if (t_class == "_top" || t_class == "_bottom") { t_class = "_bottom" } else { t_class = t_class + "_bottom" } arrow_top = -12; marg_top = Math.round(top + org_height + opts.edgeOffset) } if (t_class == "_right_top" || t_class == "_left_top") { marg_top = marg_top + 5 } else if (t_class == "_right_bottom" || t_class == "_left_bottom") { marg_top = marg_top - 5 } if (t_class == "_left_top" || t_class == "_left_bottom") { marg_left = marg_left + 5 } tiptip_arrow.css({ "margin-left": arrow_left + "px", "margin-top": arrow_top + "px" }); tiptip_holder.css({ "margin-left": marg_left + "px", "margin-top": marg_top + "px" }).attr("class", "tip" + t_class); if (timeout) { clearTimeout(timeout) } timeout = setTimeout(function () { tiptip_holder.stop(true, true).fadeIn(opts.fadeIn) }, opts.delay) } function deactive_tiptip() { opts.exit.call(this); if (timeout) { clearTimeout(timeout) } tiptip_holder.fadeOut(opts.fadeOut) } } }) } })(jQuery);

$(function () {

    // Convert text in .validation-summary-errors
    $(".validation-summary-errors").find("li").each(function () {
        var $this = $(this);
        $this.html($this.text());
    });

    // Init tipTip
    if ($.fn.tipTip) $(".tlp").each(function () { $(this).tipTip({ maxWidth: "auto" }) });

    // Init toolTip
    if ($.fn.tooltip) $('.icon_silk_help').tooltip();

    // Init timeago
    if ($.fn.timeago) $("abbr.timeago").timeago();

});

// ajax loading tab content
$('.ajax-tabs a').click(function (e) {
    if ($($(this).attr("href")).find('.overlay-content-property').length > 0)
        $($(this).attr("href")).load($(this).attr("data-url"), function () { fixPagerTop() });
    e.preventDefault();
    $(this).tab('show');
})

// ajax loading paging
$('.ajax-paging').on('click', '.pager a', function () {
    var container = $(this).closest('.ajax-paging');
    var article = $(this).closest('article.content-item');
    if (article.length > 0) $('html, body').animate({ scrollTop: article.offset().top }, 500);
    container.load($(this).attr('href'), function () { fixPagerTop() });
    return false;
})

function fixPagerTop() {    $('.pager-top').each(function () {$(this).html($(this).siblings('.pager-bottom').html()) })};

// URL Params
function getURLParameter(name) { return decodeURI((RegExp(name + '=' + '(.+?)(&|$)').exec(location.search) || [, null])[1]); }

function removeByValue(arr, val) { for (var i = 0; i < arr.length; i++) { if (arr[i] == val) { arr.splice(i, 1); break; } } }

$(document).ready(function () {

    // Load activeTab content
    $('.ajax-tabs .active a').click();

    

    // Fixed pager link with Array params
    $('.pager a').each(function () {
        var strDomain = this.href.substring(0, this.href.indexOf('?') + 1);
        var strParams = this.href.substring(this.href.indexOf('?') + 1);
        var arrParams = strParams.split('&');
        var arrToAdd = new Array();
        var arrToRemove = new Array();
        for (var i = 0; i < arrParams.length; i++) {
            if (arrParams[i].indexOf('%2C') > 0) {
                arrToRemove.push(arrParams[i]);
                var pos = arrParams[i].indexOf('=');
                var key = arrParams[i].substring(0, pos);
                var val = arrParams[i].substring(pos + 1);
                var arrVal = val.replace(/%2C/gi, ',').split(',');
                for (var j = 0; j < arrVal.length; j++) {
                    arrToAdd.push(key + '=' + arrVal[j]);
                }
            }
        }
        for (var i = 0; i < arrToRemove.length; i++) {
            removeByValue(arrParams, arrToRemove[i]);
        }
        arrParams = arrParams.concat(arrToAdd);
        $(this).attr('href', strDomain + arrParams.join('&'));
    });

    fixPagerTop();

    $(".marquee").each(function () {
        $(this).css({ "overflow": "hidden", "width": "100%" });

        // wrap "My Text" with a span (IE doesn't like divs inline-block)
        $(this).wrapInner("<span>");
        $(this).find("span").css({ "width": "50%", "display": "inline-block", "text-align": "center" });
        $(this).append($(this).find("span").clone()); // now there are two spans with "My Text"

        $(this).wrapInner("<div>");
        $(this).find("div").css("width", "210%");

        var reset = function () {
            $(this).css("margin-left", "0%");
            $(this).animate({ "margin-left": "-100%" }, 15000, 'linear', reset);
        };

        reset.call($(this).find("div"));
    });
});

//RealEstate.UserControlpanel
function ajaxExcuteCollection(flag)
{
    var dataArray = [];
    $('input:checkbox[id^=Properties]').filter(':checked').each(function () {
        dataArray.push($(this).prev().val());
    });
    if (dataArray.length > 0) {
        $.ajax({
            type: "post",
            dataType: "",
            url: "/ajax/realestate.usercontrolpanel/ajaxExcuteCollection",
            data: {
                Properties: dataArray.join(','), flag: flag,
                __RequestVerificationToken: antiForgeryToken
            },
            success: function (response) {
                if (response.status) {
                    switch(flag)
                    {
                        case 1 , 5:
                            alert('Đã xóa thành công!');
                            break;
                        case 2:
                            alert('Đã làm mới thành công!');
                            break;
                        case 3:
                            alert('Đã ngừng đăng các tin đã chọn!');
                            break;
                        case 4:
                            alert('Đã đăng lại các tin đã chọn!');
                            break;
                        case 6:
                            alert('Đã khôi phục thành công!');
                            break;
                        case 7:
                            alert('Đã xóa thành công tin theo dõi!');
                            break;
                    }
                    window.location.reload();
                }
                else
                    console.log(response.message);
            },
            error: function (request, status, error) {
                console.log(error);
            }
        });
    } else {
        alert('Vui lòng chọn tin cần thực hiện!');
    }
}

$(function () {
    $('#panelSearch').submit(function (event) {
        // Stop form from submitting normally
        event.preventDefault();
        //
        $("#overlay").show();
        var $form = $(this);
        var $returnAdsType = $('#FrmReturnStatus').val();
        //alert($form.serialize());

        $.post("/ajax/realestate.usercontrolpanel/ajaxPanelSearch", $form.serialize())
        .done(function (data) {
            switch ($returnAdsType) {
                case "ad-selling":
                    $('#ViewIndex-ad-selling').html(data);
                    break;
                case "ad-leasing":
                    $('#ViewIndex-ad-leasing').html(data);
                    break;
                case "ad-buying":
                    $('#ViewIndex-ad-buying').html(data);
                    break;
                case "ad-renting":
                    $('#ViewIndex-ad-renting').html(data);
                    break;
                default:
                    $('#ViewIndex').html(data);
                    break;
            }
            $("#overlay").hide();
        });
    });
    //
    $('#formPostVIP').submit(function (event) {
        // Stop form from submitting normally
        event.preventDefault();
        //
        $("#overlay").show();
        var $form = $(this);
        //alert($form.serialize());
        $.ajax({
            type: "post",
            dataType: "",
            url: "/ajax/realestate.usercontrolpanel/AjaxSubmitPostVIP",
            data: $form.serialize(),
                __RequestVerificationToken: antiForgeryToken,
            success: function (response) {
                if (response.status) {
                    alert('Đã đăng tin VIP ' + +response.vip + ' thành công!');
                    $('#PostVIPModal').modal('hide');
                    window.location.reload();
                }
                else
                    alert('Lỗi: ' + response.message);
            },
            error: function (request, status, error) {
                console.log(error);
            }
        });
        //$("#overlay").hide();
    });
    //Xóa tin nhắn
});
function AjaxDeleteMessage(Id,$that) {
    if (confirm('Bạn có chắc không')) {
        $.ajax({
            type: "post",
            dataType: "",
            url: "/ajax/control-panel/AjaxDeleteMessage",
            data: { Id: Id, __RequestVerificationToken: antiForgeryToken },
            success: function (response) {
                if (response.status) {
                    window.location.reload();
                }
                else
                    alert('Lỗi: ' + response.message);
            },
            error: function (request, status, error) {
                console.log(error);
            }
        });
    }
}