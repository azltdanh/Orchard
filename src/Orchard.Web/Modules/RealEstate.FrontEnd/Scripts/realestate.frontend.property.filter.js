// Init
minScreen = 767;
// customize message for jquery validate
$("#AdsTypeCssClass").attr("data-msg-required", "Bạn phải chọn loại giao dịch");
$("#TypeGroupId").attr("data-msg-required", "Bạn phải chọn loại bất động sản");
$("#TypeGroupCssClass").attr("data-msg-required", "Bạn phải chọn nhóm bất động sản");
$("#ExchangeTypeClass").attr("data-msg-required", "Bạn phải chọn loại yêu cầu trao đổi");
$("#TypeId").attr("data-msg-required", "Bạn phải chọn loại bất động sản");
$("#TypeConstructionId").attr("data-msg-required", "Bạn phải chọn loại công trình xây dựng");

$("#ProvinceId,[name$=ProvinceId]").attr("data-msg-required", "Bạn phải chọn Tỉnh / Thành Phố");
$("#DistrictId,[name$=DistrictId]").attr("data-msg-required", "Bạn phải chọn Quận / Huyện");
$("#WardId,[name$=WardId]").attr("data-msg-required", "Bạn phải chọn Phường / Xã");
$("#StreetId,[name$=StreetId]").attr("data-msg-required", "Bạn phải chọn Đường / Phố");
$("#AddressNumber").attr("data-msg-required", "Bạn phải nhập Số nhà");
$("#OtherProjectName").attr("data-msg-required", "Bạn phải nhập Tên dự án / chung cư");

$("#LocationCssClass").attr("data-msg-required", "Bạn phải chọn vị trí BĐS");
$("#DistanceToStreet,#AlleyTurns,#AlleyWidth1,#AlleyWidth2,#AlleyWidth3,#AlleyWidth4,#AlleyWidth5,#AlleyWidth6,#AlleyWidth7,#AlleyWidth8,#AlleyWidth9,#AreaUsable").attr("data-msg-required", "Vui lòng nhập");

$('#AreaTotalWidth').attr("data-msg-required", "Bạn phải nhập chiều ngang khuôn viên đất.");
$('#AreaTotalLength').attr("data-msg-required", "Bạn phải nhập chiều dài khuôn viên đất.");
$('#AreaLegalWidth').attr("data-msg-required", "Bạn phải nhập chiều ngang phù hợp quy hoạch.");
$('#AreaLegalLength').attr("data-msg-required", "Bạn phải nhập chiều dài phù hợp quy hoạch.");

$("#ContactPhone").attr("data-msg-required", "Bạn phải nhập tên + điện thoại");
$("#Name").attr("data-msg-required", "Bạn phải nhập tên");
$("#Phone").attr("data-msg-required", "Bạn phải nhập điện thoại");

// customize message for jquery validate
// TODO: move this dialog to new view
$("#ContactName").attr("data-msg-required", "Bạn phải nhập Họ tên");
$("#ContactPhone").attr("data-msg-required", "Bạn phải nhập Số điện thoại");
$("#ContactEmail").attr("data-msg-required", "Bạn phải nhập Địa chỉ Email");

$('#search-all .district button').hover(function () {
    if ($('#search-all [name$=ProvinceId]').val() == '') {
        $("#search-all .district .er-active").toggle();
    }
});
$('#search-all .ward button').hover(function () {
    if ($('#search-all #DistrictIds :selected').length < 1) {
        $("#search-all .ward .er-active").toggle();
    }
});
$('#search-all .street button').hover(function () {
    if ($('#search-all #DistrictIds :selected').length < 1) {
        $("#search-all .street .er-active").toggle();
    }
});


$('#search-items .district button').hover(function () {
    if ($('#search-items [name$=ProvinceId]').val() == '') {
        $("#search-items .district .er-active").toggle();
    }
});
$('#search-items .ward button').hover(function () {
    if ($('#search-items #DistrictIds :selected').length < 1) {
        $("#search-items .ward .er-active").toggle();
    }
});
$('#search-items .street button').hover(function () {
    if ($('#search-items #DistrictIds :selected').length < 1) {
        $("#search-items .street .er-active").toggle();
    }
});
$('#search-items .apartment button').hover(function () {
    if ($('#search-items #DistrictIds :selected').length < 1) {
        $("#search-items .apartment .er-active").toggle();
    }
});


//Get Param Url
$.extend({
    getUrlVars: function () {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    },
    getUrlVar: function (name) {
        return $.getUrlVars()[name];
    }
});
//use: $.getUrlVar('param')


$("#search-all #AdsTypeCssClass").change(function () {
    // Filter
    // Chuyển đổi phương thức thanh toán thành Triệu VND khi tìm kiếm Thuê và Cho Thuê
    if ($(this).val() === "ad-leasing" || $(this).val() === "ad-renting") {
        $('#search-all #PaymentMethodCssClass').val('pm-vnd-m');
    }
    else {
        $('#search-all #PaymentMethodCssClass').val('pm-vnd-b');
    }
});

$("#AdsTypeCssClass").change(function () {
    // Filter
    // Chuyển đổi phương thức thanh toán thành Triệu VND khi tìm kiếm Thuê và Cho Thuê
    if ($(this).val() === "ad-leasing" || $(this).val() === "ad-renting") {
        $('#PaymentMethodCssClass').val('pm-vnd-m');
    }
    else {
        $('#PaymentMethodCssClass').val('pm-vnd-b');
    }
});

$("#TypeGroupCssClass").change(function () {
    initFormFilter();
});

// Auto-Format number input
if ($.fn.autoNumeric) {
    $(".AutoFloat3").autoNumeric('init', { aPad: false, vMax: '999' });
    $(".AutoFloat4").autoNumeric('init', { aPad: false, vMax: '9999' });
    $(".AutoFloat7").autoNumeric('init', { aPad: false, vMax: '9999999' });
    $(".AutoInt2").autoNumeric('init', { aPad: false, vMin: '0', vMax: '99' });
    $(".AutoInt4").autoNumeric('init', { aPad: false, vMin: '0', vMax: '9999' });
    $('input[type="submit"], button[type="submit"]').click(function () {
        $('input.AutoFloat3, input.AutoFloat4, input.AutoFloat7, input.AutoInt2, input.AutoInt4').each(function () { if ($(this).val().length > 0) { $(this).val($(this).autoNumeric('get')); } });
    });
}

var typegroup;

//tabs Search Control
$('.btnSearchControl').click(function () {
  
    var form = $(this).closest('form');

    $('input[type=text], input[type=email], input[type=hidden], select, button[type=submit]', form).each(function () { this.disabled = ($(this).val() === 'false' || $(this).val() === ''); });

    $('input[type=checkbox]', form).each(function () { this.disabled = !($(this).is(':checked')); });

   $('#PaymentMethodCssClass', form).prop('disabled', !($('#MinPriceProposed').val() > 0 || $('#MaxPriceProposed').val() > 0));

    $('input[name^=autocomplete]').prop('disabled', true);
    $('input[name^=autocompleteApartmentProvinceId]').prop('disabled', true);

    
    //$('input[name="__RequestVerificationToken"]', form).prop('disabled', true);
    form.submit();
});

//Tab control
$('#gp-house').on('click', function (e) {
    $('#TypeGroupCssClass').val('gp-house');
    refreshform(e);
    InitTabGBHouse();
    initFormFilter();
});
$('#gp-apartment').on('click', function (e) {
    $('#TypeGroupCssClass').val('gp-apartment');
    refreshform(e);
    InitTabGBApartment();
    initFormFilter();
});
$('#gp-land').on('click', function (e) {
    $('#TypeGroupCssClass').val('gp-land');
    refreshform(e);
    InitTabGBLand();
    initFormFilter();
});
$('#gp-all').on('click', function (e) {
    $('#TypeGroupCssClass').val('');
    refreshform(e);
    InitTabGBAll();
    initFormFilter();
});

// Subscribe Newsletter
$('#mySubscribe [type=submit]').click(function () {
    if ($('#TypeGroupCssClass').val() === '' || $('#ProvinceId').val() === '') {
        $('#alertRegister').show();
        return false;
    }
    else {
        $(this).closest('form').attr("action", $(this).attr('data-url'));
    }
});
$('#mySubscribe').on('show', function () {
    getContactInfo();
    $('#mySubscribe').unbind('show');
});
$('#mySubscribe').on('hidden', function () {
    $('#alertRegister').hide();
});

// Load thông tin liên hệ current user
function getContactInfo() {
    // TODO: use cookie, create new when user login, delete when user logout
    $.ajax({
        type: "post",
        dataType: "",
        url: "/RealEstate.UserControlPanel/User/GetProfile",
        data: {
            __RequestVerificationToken: antiForgeryToken
        },
        success: function (response) {
            if (response.success == true) {
                if ($("#ContactName").length > 0) { if ($("#ContactName").val().length == 0 && response._name.length > 0) { $("#ContactName").val(response._name); } }
                if ($("#ContactPhone").length > 0) { if ($("#ContactPhone").val().length == 0 && response._number.length > 0) { $("#ContactPhone").val((response._name.length > 0 ? (response._name + ' ') : '') + response._number); } }
                if ($("#ContactEmail").length > 0) { if ($("#ContactEmail").val().length == 0 && response._email.length > 0) { $("#ContactEmail").val(response._email); } }
                if ($("#ContactAddress").length > 0) { if ($("#ContactAddress").val().length == 0 && response._address.length > 0) { $("#ContactAddress").val(response._address); } }
            }
        },
        error: function (request, status, error) {
        }
    });
}

$(function () {

    $(".linksaveproperty").on("click", function () {
        $.ajax({
            type: "post",
            dataType: "JSON",
            traditional: true,
            url: "/ajax/RealEstate.FrontEnd/PropertySearch/SaveProperty",
            data: {
                propertyid: $(this).attr('href').replace('#', ''),
                __RequestVerificationToken: antiForgeryToken
            },
            success: function (response) {
                if (response.success == true) {
                    alert("Lưu tin thành công. Xem các tin đã lưu bạn vào Trang quản lý -> Quản lý tin rao -> BĐS lưu theo dõi");
                }
                else {
                    if (response.flaguser == true) {
                        alert("Tin này đã được lưu.");
                    }
                    else {
                        //window.location.href = '@Url.Action("AccessDenied", "Account", new { area = "Orchard.Users", ReturnUrl = HttpContext.Current.Request.Url })';
                    }
                }
            },
            error: function (request, status, error) {
            }
        });
        return false;
    });

    // Province change - If Hà Nội -> show AddressCorner
    typegroup = $('#tab-control-filter').val();
    function toggleAddressCorner() {
        if ($('#ProvinceId', $('#AddressCorner').closest('form')).children('option:selected').text() == 'Hà Nội') {
            $('#AddressCorner').closest('.form-group').show();
        }
        else {
            $('#AddressCorner').closest('.form-group').hide();
        }
    };
    toggleAddressCorner();
    $('#ProvinceId', $('#AddressCorner').closest('form')).change(function () {
        toggleAddressCorner();
    });

    if ($.fn.validate) {
        var validator = $(".formvalidate").validate({
            errorClass: 'text-error'
        });
    }
    // Giới hạn số lượng ký tự nhập vào
    if ($.fn.limiter) {
        if ($("#charsNote").length != 0) { var elemNote = $("#charsNote"); $("#Note").limiter(500, elemNote); }
    }

    //refresh control in tab search-all
    $("#search-all #btnrefresh").click(function (e) {
        var typegroup = $('#TypeGroupCssClass').val();
        refreshform(e);
        if (typegroup == 'gp-house') {
            $('#gp-house').click();
        }
        else if (typegroup == 'gp-apartment') {
            $('#gp-apartment').click();
        }
        else if (typegroup == 'gp-land') {
            $('#gp-land').click();
        }
        else {
            $('#gp-all').click();
        }
        return false;
    });

    //refresh control  in tab search-items
    $("#btnrefresh").click(function (e) {
        var typegroup = $('#TypeGroupCssClass').val();
        refreshform(e);
        if (typegroup == 'gp-house') {
            $('#gp-house').click();
        }
        else if (typegroup == 'gp-apartment') {
            $('#gp-apartment').click();
        }
        else if (typegroup == 'gp-land') {
            $('#gp-land').click();
        }
        else {
            $('#gp-all').click();
        }
        return false;
    });

    if ($('.form-filter-header').is(':visible')) {
        if (typegroup == 'gp-house') {
            $('.form-filter-header #TypeGroupCssClass').val('gp-house');
        }
        else if (typegroup == "gp-apartment") {
            $('.form-filter-header #TypeGroupCssClass').val('gp-apartment');
        }
        else if (typegroup == "gp-land") {
            $('.form-filter-header #TypeGroupCssClass').val('gp-land');
        }
        else {
            $('.form-filter-header #TypeGroupCssClass').val("");
        }
    }

    initFormFilter();

    $('#formFilter input,#formFilter select').change(function () { initFormFilter(); });

    // checked for anytype.
    if ($.getUrlVar('AdsGoodDeal') == 'true') { $('#AdsGoodDeal').prop('checked', true); }
    if ($.getUrlVar('AdsVIP') == 'true') { $('#AdsVIP').prop('checked', true); }
    if ($.getUrlVar('IsOwner') == 'true') { $('#IsOwner').prop('checked', true); }
    if ($.getUrlVar('IsAuction') == 'true') { $('#IsAuction').prop('checked', true); }
    /*if ($.getUrlVar('AllAnyType') == 1) {
        $('#AllAnyType').prop('checked', true);
        $('#AdsGoodDeal').prop('checked', false);
        $('#AdsVIP').prop('checked', false);
        $('#IsOwner').prop('checked', false);
        $('#IsAuction').prop('checked', false);
    }
    if ($('#AdsGoodDeal').is(':checked') || $('#AdsVIP').is(':checked') || $('#IsOwner').is(':checked') || $('#IsAuction').is(':checked')) {
        $('#AllAnyType').prop('checked', false);
    }
    // click
    $('#AdsGoodDeal').click(function () { if ($('#AllAnyType').is(':checked')) { $('#AllAnyType').attr('checked', false); } });
    $('#AdsVIP').click(function () { if ($('#AllAnyType').is(':checked')) { $('#AllAnyType').attr('checked', false); } });
    $('#IsOwner').click(function () { if ($('#AllAnyType').is(':checked')) { $('#AllAnyType').attr('checked', false); } });
    $('#IsAuction').click(function () { if ($('#AllAnyType').is(':checked')) { $('#AllAnyType').attr('checked', false); } });
    $('#AllAnyType').click(function () { if ($('#AllAnyType').is(':checked')) { $('#AdsGoodDeal, #AdsVIP, #IsOwner, #IsAuction').attr('checked', false); } });
    */
    // ChkOtherProjectName
    $('#ChkOtherProjectName').change(function () {
        var slcApartmentIds = $("#ApartmentIds", $(this).closest('form'));
        if ($(this).is(':checked')) {
            slcApartmentIds.siblings('.btn-group.form-control').hide();
            slcApartmentIds.empty();
            slcApartmentIds.multiselect('rebuild');
            $('#OtherProjectName').show();
        } else {
            slcApartmentIds.siblings('.btn-group.form-control').show();
            $('#OtherProjectName').hide();
        }
    });
    if ($(window).width() < 992) {// kiem tra kich thuoc man hinh cua thiet bi
        $('.row').find('.two-column-first').removeClass('two-column-first');
        $('.row').find('.two-column-last').removeClass('two-column-last');
    }
    if ($(window).width() <= 767) {
        $('#frmHeaderFilter').remove();
    }
    // Advance Search
    if ($.cookie('advance-search-expand-frontend') == 'true') {
        $('.bt-expand-search').find('.ex-icon').addClass('icon_green_arrow_up');
        $('.bt-expand-search').find('.ex-icon').addClass('icons-expand');
        $('.advance-search').show();
        if ($(window).width() < minScreen) {
            $('#WardIds, #StreetIds, #DirectionIds').closest('.form-group').show();
        }
    }
    else {
        $('.advance-search').hide();
        if ($(window).width() < minScreen) {
            $('#WardIds, #StreetIds, #DirectionIds').closest('.form-group').hide();
        }
        $('.bt-expand-search').find('.ex-icon').addClass('icon_green_arrow_down');
        $('.bt-expand-search').find('.ex-icon').addClass('icons-collapse');
    }

    $('.bt-expand-search').click(function () {
        $(this).find('.ex-icon').toggleClass('icon_green_arrow_down');
        $(this).find('.ex-icon').toggleClass('icons-collapse');
        $('.advance-search').toggle();
        if ($(window).width() < minScreen) {
            $('#WardIds, #StreetIds, #DirectionIds').closest('.form-group').toggle();
        }
        $.cookie('advance-search-expand-frontend', !($.cookie('advance-search-expand-frontend') == 'true'));
    });
    //Interval
    var sellingId = setInterval(function () {
        $('#NewSelector .waiting-selling-content').each(function (e) {
            var $that = $(this);
            if ($($that).find('.overlay-content-property').length > 0) {
                $($that).load($($that).attr("data-url"), function () { fixPagerTop() });
            }
        });
        setTimeout(function () {
            clearInterval(sellingId);
        }, 1000);
    }, 5000);
    //var buyingId = setInterval(function () {
    //        $('#NewSelector .waiting-buying-content').each(function (e) {
    //            var $that = $(this);
    //            if ($($that).find('.overlay-content-property').length > 0) {
    //                $($that).load($($that).attr("data-url"), function () { fixPagerTop() });
    //            }
    //        });
    //        setTimeout(function () {
    //            clearInterval(buyingId);
    //        }, 1000);
    //    }, 10000);
});

function refreshform(e) {
    //AnyType
    $("#AdsGoodDeal").prop('checked', false);
    $("#AdsVIP").prop('checked', false);
    $("#IsOwner").prop('checked', false);
    $("#IsAuction").prop('checked', false);

    $("#search-all #AdsGoodDeal").prop('checked', false);
    $("#search-all #AdsVIP").prop('checked', false);
    $("#search-all #IsOwner").prop('checked', false);
    $("#search-all #IsAuction").prop('checked', false);
    //value change
    //$("#AdsTypeCssClass").val('').change();
    $("#TypeGroupCssClass").val('').change();
    $("#DirectionIds").val('');
    $("#TypeIds").val('');
    $("#ApartmentFloorTh").val("").change();
    //AlleyTurnsRange
    $("#AlleyTurnsRange").val("").change();
    $("#ProvinceId").val(0).combobox("refresh").change();
    $('#search-items [name$=ProvinceId]').val('');
    $('#search-all [name$=ProvinceId]').val('');
  //  $("#PaymentMethodCssClass").val('').change();
    $('input[type="text"]').val("");

    $("#DistrictIds").empty();
    $(".district .multiselect-container").empty();
    $(".district .multiselect").html('Chọn tất cả Quận / Huyện <b class="caret"></b>');

    $("#WardIds").empty();
    $(".ward .multiselect-container").empty();
    $(".ward .multiselect").html('Chọn tất cả Phường / Xã  <b class="caret"></b>');

    $("#StreetIds").empty();
    $(".street .multiselect-container").empty();
    $(".street .multiselect").html('Chọn tất cả các Đường <b class="caret"></b>');

    $(".apartment .multiselect-container").empty();
    $(".apartment .multiselect").html('Chọn tất cả dự án / chung cư <b class="caret"></b>');

    multiselect_deselectAll($("#search-items #DirectionIds"));
    multiselect_deselectAll($("#search-all #DirectionIds"));
    multiselect_deselectAll($("#search-all #TypeIds"));

    //not show land , house, all
    e.preventDefault();
}
function initFormFilter() {
    // $('#DistrictIds').closest('.form-group').toggle($('#ProvinceId').val() > 0);
    //$('#TypeGroupCssClass').closest('.form-group').toggle($('#TypeGroupCssClass').val() > 0);
    if ($('#TypeGroupCssClass').val() == "gp-all" || $('#TypeGroupCssClass').val() == undefined || $('#TypeGroupCssClass').val() == '') {
        $("#search-items").css('display', 'none');
        $("#search-all").css('display', 'block');
    }
    else {
        $("#search-items").css('display', 'block');
        $("#search-all").css('display', 'none');

        // show on gp-apartment
        $('#OtherProjectName, #ApartmentFloorThRange, #MinBedrooms, #MinAreaUsable, #ApartmentIds').closest('.form-group').toggle($('#TypeGroupCssClass').val() == "gp-apartment");

        //not show on gp-apartment
        $('#DirectionIds, #MinAreaTotal').closest('.form-group').toggle($(window).width() > minScreen && $('#TypeGroupCssClass').val() != "gp-apartment");

        //show on gp-house, gp-land
        $('#AlleyTurnsRange, #MinAreaTotalWidth, #MinAreaTotalLength, #StreetIds, #MinLength, #MinFloors, #MinWidth').closest('.form-group').toggle($('#TypeGroupCssClass').val() == "gp-house" || $('#TypeGroupCssClass').val() == "gp-land");

        //show on gp-house
        $('#MinFloors').closest('.form-group').toggle($('#TypeGroupCssClass').val() == "gp-house");

        //show on gp-all
        $('#TypeIds').closest('.form-group').toggle($('#TypeGroupCssClass').val() == "");

        //if ($('#TypeGroupCssClass').val() == 'gp-apartment') {
        //    $('#WardIds, #DirectionIds').closest('.form-group').show();
        //    $('#ApartmentIds').closest('.form-group').show();
        //}
    }

    if ($('#TypeGroupCssClass').val() == 'gp-house') {
        InitTabGBHouse();
        $('#gp-house').closest('li').addClass('active');
        $('#gp-house').closest('li').siblings().removeClass('active');
    }
    else if ($('#TypeGroupCssClass').val() == 'gp-apartment') {
        InitTabGBApartment();
        $('#gp-apartment').closest('li').addClass('active');
        $('#gp-apartment').closest('li').siblings().removeClass('active');
    }
    else if ($('#TypeGroupCssClass').val() == 'gp-land') {
        InitTabGBLand();
        $('#gp-land').closest('li').addClass('active');
        $('#gp-land').closest('li').siblings().removeClass('active');
    }
    else if ($('#TypeGroupCssClass').val() == '') {
        InitTabGBAll();
        $('#gp-all').closest('li').addClass('active');
        $('#gp-all').closest('li').siblings().removeClass('active');
    }
    else { }
}

//remove multiselect 
function multiselect_deselectAll($el) {
    $('option', $el).each(function (element) {
        $el.multiselect('deselect', $(this).val());
    });
}
function InitTabGBHouse() {
    $('#TypeGroupCssClass').val("gp-house");
    if ($(window).width() > minScreen) {
        $('#WardIds, #StreetIds, #DirectionIds').closest('.form-group').show();
    }
}
function InitTabGBApartment() {
    $('#TypeGroupCssClass').val("gp-apartment");
    if ($(window).width() > minScreen) {
        $('#WardIds, #DirectionIds').closest('.form-group').show();
    }
    $('#ApartmentIds').closest('.form-group').show();
}
function InitTabGBLand() {
    $('#TypeGroupCssClass').val("gp-land");
    if ($(window).width() > minScreen) {
        $('#WardIds, #StreetIds, #DirectionIds').closest('.form-group').show();
    }
}
function InitTabGBAll() {
    $('#TypeGroupCssClass').val("");
    if ($(window).width() > minScreen) {
        $('#WardIds, #StreetIds, #DirectionIds').closest('.form-group').show();
    }
}

//Character Limit Counter
(function ($) { $.fn.extend({ limiter: function (limit, elem) { $(this).on("keyup focus", function () { setCount(this, elem); }); function setCount(src, elem) { var chars = src.value.length; if (chars > limit) { src.value = src.value.substr(0, limit); chars = limit; } elem.html(limit - chars); } setCount($(this)[0], elem); } }); })(jQuery);

/** ExchangeTypeCssClass **/
$('#ExchangeTypeClass').change(function () {
    var that = $(this);
    $('#Values').closest('.form-group').toggle(!($(that).val() == '' || $(that).val() == 'exchange-parity'));
    if ($(that).val() != null && $(that).val() == 'exchange-more')
    {
        $('.label-exchange').text('Số tiền bù thêm:');
    }
    if ($(that).val() != null && $(that).val() == 'exchange-lower') {
        $('.label-exchange').text('Số tiền muốn nhận thêm:');
    }
}).change();