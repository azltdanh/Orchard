var ChuSo = new Array(" không ", " một ", " hai ", " ba ", " bốn ", " năm ", " sáu ", " bảy ", " tám ", " chín ");
var Tien = new Array("", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ");

// customize message for jquery validate
$("#AdsTypeCssClass").attr("data-msg-required", "Bạn phải chọn loại giao dịch");
$("#TypeGroupId").attr("data-msg-required", "Bạn phải chọn loại bất động sản");
$("#TypeGroupCssClass").attr("data-msg-required", "Bạn phải chọn nhóm bất động sản");
$("#TypeId").attr("data-msg-required", "Bạn phải chọn loại bất động sản");
$("#TypeConstructionId").attr("data-msg-required", "Bạn phải chọn loại công trình xây dựng");
$("#FloorsCount").attr("data-msg-required", "Bạn phải chọn số lầu");

$("#ProvinceId,[name$=ProvinceId]").attr("data-msg-required", "Bạn phải chọn Tỉnh / Thành Phố");
$("#DistrictId,[name$=DistrictId]").attr("data-msg-required", "Bạn phải chọn Quận / Huyện");
$("#WardId,[name$=WardId]").attr("data-msg-required", "Bạn phải chọn Phường / Xã");
//$("#StreetId,[name$=StreetId]").attr("data-msg-required", "Bạn phải chọn Đường / Phố");
$("#AddressNumber").attr("data-msg-required", "Bạn phải nhập Số nhà");
//$("#OtherProjectName").attr("data-msg-required", "Bạn phải nhập Tên dự án / chung cư");
//$("#ApartmentId,[name$=ApartmentId]").attr("data-msg-required", "Bạn phải chọn Tên dự án / chung cư");
$("#LocationCssClass").attr("data-msg-required", "Bạn phải chọn vị trí BĐS");
$("#DistanceToStreet,#AlleyTurns,#AlleyWidth1,#AlleyWidth2,#AlleyWidth3,#AlleyWidth4,#AlleyWidth5,#AlleyWidth6,#AlleyWidth7,#AlleyWidth8,#AlleyWidth9,#AreaUsable").attr("data-msg-required", "Vui lòng nhập");

$('#AreaTotalWidth').attr("data-msg-required", "Bạn phải nhập chiều ngang khuôn viên đất hoặc chỉ cần nhập Tổng DT.");
$('#AreaTotalLength').attr("data-msg-required", "Bạn phải nhập chiều dài khuôn viên đất hoặc chỉ cần nhập Tổng DT.");
$('#AreaLegalWidth').attr("data-msg-required", "Bạn phải nhập chiều ngang phù hợp quy hoạch.");
$('#AreaLegalLength').attr("data-msg-required", "Bạn phải nhập chiều dài phù hợp quy hoạch.");


$("#ContactPhone").attr("data-msg-required", "Bạn phải nhập tên + điện thoại");

$(document).ready(function () {
    if ($.fn.validate) {
        var validator = $(".formvalidate").validate({
            errorClass: 'text-error'
        });
    }
    if ($('.show-share-face').length > 0) {
        $('.check-share-face').html($('.show-share-face'));
    }
});

$(function () {

    // #region Create / Edit Property

    if ($("#PriceProposed").val() <= 0) $("#PriceProposed").val("");

    // Province change - If Hà Nội -> show AddressCorner
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

    // Toggle OtherWardName and OtherStreetName
    $('#ChkOtherDistrictName').change(function () {
        if ($(this).is(':checked')) { $('#DistrictId').val('').change().next().hide().next().hide(); $('#OtherDistrictName').show(); $('#ChkOtherWardName').attr('checked', true).change(); $('#ChkOtherStreetName').attr('checked', true).change(); } else { $('#DistrictId').val('').change().next().show().next().show(); $('#OtherDistrictName').hide(); $('#ChkOtherWardName').attr('checked', false).change(); $('#ChkOtherStreetName').attr('checked', false).change(); }
        //$(this).closest("form").validate().element($('#DistrictId'));
        //$(this).closest("form").validate().element($('#OtherDistrictName'));
    });
    $('#ChkOtherWardName').change(function () {
        if ($(this).is(':checked')) { $('input[name="WardId"]').val('').next().hide().next().hide(); $('#OtherWardName').show(); } else { $('input[name="WardId"]').val('').next().show().next().show(); $('#OtherWardName').hide(); }
        //$(this).closest("form").validate().element($('#WardId'));
        //$(this).closest("form").validate().element($('#OtherWardName'));
    });
    $('#ChkOtherStreetName').change(function () {
        if ($(this).is(':checked')) { $('input[name="StreetId"]').val('').next().hide().next().hide(); $('#OtherStreetName').show(); } else { $('input[name="StreetId"]').val('').next().show().next().show(); $('#OtherStreetName').hide(); }
        //$(this).closest("form").validate().element($('#StreetId'));
        //$(this).closest("form").validate().element($('#OtherStreetName'));
    });
    $('#ChkOtherProjectName').change(function () {
        if ($(this).is(':checked')) { $('input[name="ApartmentId"]').val('').next().hide().next().hide(); $('#OtherProjectName').show(); } else { $('input[name="ApartmentId"]').val('').next().show().next().show(); $('#OtherProjectName').hide(); }
        //$(this).closest("form").validate().element($('#StreetId'));
        //$(this).closest("form").validate().element($('#OtherStreetName'));
    });

    // Apartment change - load Apartment info and fill in form
    $('#ApartmentId').change(function () {
        var slcWard = $("#" + this.id.replace("ApartmentId", "WardId"), $(this).closest('form'));
        var slcStreet = $("#" + this.id.replace("ApartmentId", "StreetId"), $(this).closest('form'));
        if ($(this).val() > 0) {
            var apartmentId = $(this).val();
            var oldApartmentId = $('#oldAprartmentId').val();
            var propertyId = $('#Id').text();
            $.ajax({
                type: "get",
                dataType: "",
                url: "/RealEstate.Admin/Home/GetApartmentInfoForJson",
                data: {
                    apartmentId: $(this).val(), propertyId: propertyId
                },
                success: function (response) {
                    $('.apartment_' + oldApartmentId).remove(); $('#oldAprartmentId').val(apartmentId);
                    $.each(response.apartmentList, function (i, item) {
                        if (item.Path != '') {
                            var checkedAvatar = '';
                            var fileName = item.Path.split('/');
                            if (item.IsAvatar) checkedAvatar = 'checked=\"checked\"';
                            $(".picture_house").append(
                                '<li class="managerimages apartment_' + apartmentId + '" data-id="' + item.Id + '" data-property-id="' + propertyId + '"> \
                                    <img alt="' + fileName[fileName.length - 1] + '" src="' + item.Path + '?width=100&amp;height=125" width="110" class="img-thumbnail" /> \
                                    <div class="checkbox"> \
                                        <label title="Cho phép hiện hình"><input type="checkbox" name="imgPublished" checked="checked" />Hiện&nbsp;&nbsp;</label> \
                                    </div> \
                                    <div class="radio"> \
                                        <label><input type="radio" name="avatar" ' + checkedAvatar + '>Ảnh Chính</label> \
                                    </div> \
                                </li>');
                        }
                    });

                    if (response.apartmentWardId == 0)
                        slcWard.siblings('.combobox-container').children('input').val('');
                    else
                        $('#WardId').val(response.apartmentWardId).combobox('refresh').change();

                    if (response.apartmentStreetId == 0)
                        slcStreet.siblings('.combobox-container').children('input').val('');
                    else
                        $('#StreetId').val(response.apartmentStreetId).combobox('refresh').change();
                    $('#AddressNumber').val(response.apartmentAddressNumber);

                    $('#ApartmentFloors').val(response.apartmentFloors);
                    $('#ApartmentElevators').val(response.apartmentElevators);
                    $('#ApartmentBasements').val(response.apartmentBasements);
                    $('#Content').val(response.apartmentDescription);
                    $('#ApartmentTradeFloors').val(response.apartmentTradeFloors);

                    $('.apartment-adv-Childcare').prop('checked', response.apartmentHaveChildcare);
                    $('.apartment-adv-Park').prop('checked', response.apartmentHavePark);
                    $('.apartment-adv-SwimmingPool').prop('checked', response.apartmentHaveSwimmingPool);
                    $('.apartment-adv-SuperMarket').prop('checked', response.apartmentHaveSuperMarket);
                    $('.apartment-adv-SportCenter').prop('checked', response.apartmentHaveSportCenter);
                    $('.apartment-adv-Hospital').prop('checked', response.apartmentHaveHospital);
                    $('.apartment-adv-PublicArea').prop('checked', response.apartmentHavePublicArea);
                },
                error: function (request, status, error) {
                    $('#ApartmentFloors,#ApartmentElevators,#ApartmentBasements').val('');
                    $('.apartment-adv-Childcare,.apartment-adv-Park,.apartment-adv-SwimmingPool,.apartment-adv-SuperMarket,.apartment-adv-SportCenter').prop('checked', false);
                    console.log(error);
                }
            });
        }
    }).change();

    function updateOrtherName() {
        if ($('#ChkOtherProvinceName').is(':checked')) { $('#ProvinceId').val('').change().next().hide().next().hide(); $('#OtherProvinceName').show(); }
        if ($('#ChkOtherDistrictName').is(':checked')) { $('#DistrictId').next().hide().next().hide(); $('#OtherDistrictName').show(); }
        if ($('#ChkOtherWardName').is(':checked')) { $('input[name="WardId"]').next().hide().next().hide(); $('#OtherWardName').show(); }
        if ($('#ChkOtherStreetName').is(':checked')) { $('input[name="StreetId"]').next().hide().next().hide(); $('#OtherStreetName').show(); }
        if ($('#ChkOtherProjectName').is(':checked')) { $('input[name="ApartmentId"]').next().hide().next().hide(); $('#OtherProjectName').show(); }
    };
    updateOrtherName();


    $("#AdsTypeCssClass").change(function () {
        $('#PaymentMethodId option').removeAttr('selected');
        if ($(this).val() == "ad-leasing" || $(this).val() == "ad-renting") {
            $('#PaymentMethodId option:contains(Triệu):first').attr('selected', 'selected');
            $('#PaymentMethodCssClass').val('pm-vnd-m');
        }
        else {
            $('#PaymentMethodId option:contains(Tỷ)').attr('selected', 'selected');
            $('#PaymentMethodCssClass').val('pm-vnd-b');
        }
        if ($('#TypeGroupCssClass').val() == "gp-house") {
            loadTypeCssClass();
        }
    }).change();

    $("#TypeGroupCssClass").change(function () {
        if ($("#TypeGroupCssClass").val() != '') {
            loadTypeCssClass();
        }
        if ($('#TypeGroupCssClass').val() == 'gp-apartment') {
            $('#AreaUsable').removeAttr('required');
            $('.not-require').remove();
        }
    }).change();

    $('#TypeId').change(function () {
        var typeId = $(this).val();
        if (typeId == 231 || typeId == '') {
            // 231 - 'tp-residential-land'
            // Đất thổ cư --> không có Thông tin xây dựng
            $(".constructionInfo").hide();
        }
        else {
            $(".constructionInfo").show();
            if ($(this).val() != 238) {//238 = tp-office-building
                $("#FloorsCount").find('option:first').val(0);
                $("#FloorsCount").removeAttr('required');
                $('#FloorsCount').closest('.form-group').children('label').text('Số lầu: ');
            }
            else {
                $("#FloorsCount").find('option:first').val('');
                $("#FloorsCount").attr('required', 'required');
                $('#FloorsCount').closest('.form-group').children('label').prepend('<span class="text-error">*</span>');
            }
        }
        if (typeId > 0 && $("#CurrentTypeGroupCssClass").length > 0) {
            if ($("#TypeGroupCssClass").val() != $("#CurrentTypeGroupCssClass").text()) {
                $("[name='submit.Save']").click();
            }
        }
        // Load TypeConstructions
        $('#Floors').change();
        //$('#AreaLegalLength').change();
        floorsCountChange();//Ready when load page
    }).change();

    $('#FloorsCount').change(function () {
        floorsCountChange();

        var areaConstruction = $("#AreaConstruction", $(this).closest('form'));
        var areaConstructionFloor = $("#AreaConstructionFloor", $(this).closest('form'));
        var area = AreaContructionChange($(this));
        if (area != 0) {
            areaConstructionFloor.val(area);
            areaConstruction.change();
        }
    });

    function floorsCountChange() {
        var floorCount = $("#FloorsCount");
        if ($(floorCount).val() == -1) {
            $('#Floors').closest('.form-group').show();
            $('#Floors').focus();
        }
        else {
            $('#Floors').val($(floorCount).val()).closest('.form-group').hide();
        }
        $('#Floors').change();
    }

    $('#Floors').change(function () {
        $.ajax({
            type: "get",
            dataType: "",
            url: "/RealEstate.Admin/Home/GetPropertyTypeConstructionsForJson",
            data: {
                typeId: $('#TypeId').val(),
                floors: $('#Floors').val()
            },
            success: function (response) {
                var selectedValue = $('#TypeConstructionId').val();
                if (selectedValue == '') {
                    selectedValue = $('#TypeConstructionId').data('autofill');
                }
                $('#TypeConstructionId').empty().append("<option value=''>-- Vui lòng chọn --</option>");
                $.each(response.list, function (i, item) { $('#TypeConstructionId').append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
                $('#TypeConstructionId').val(selectedValue).change();
            },
            error: function (request, status, error) {
                $('#TypeConstructionId').empty().append("<option value=''>-- Vui lòng chọn --</option>");
                console.log(error);
            }
        });
    });

    // Location change
    $('#LocationCssClass').change(function () {
        switch ($(this).val()) {
            // Nhà MT                                                                   
            case "h-front":

                $('#StreetWidth').removeAttr('disabled');
                $('#divStreetWidth').show();

                $('#DistanceToStreet').attr('disabled', 'disabled');
                $('#divDistanceToStreet').hide();

                $('#AlleyTurns').attr('disabled', 'disabled');
                $('#divAlleyTurns').hide();

                $('input[id^="AlleyWidth"]').attr('disabled', 'disabled');
                $('[id^="divAlleyWidth"]').hide();

                break;

                // Nhà trong hẻm                                                                  
            case "h-alley":

                $('#StreetWidth').attr('disabled', 'disabled');
                $('#divStreetWidth').hide();

                $('#DistanceToStreet').removeAttr('disabled');
                $('#divDistanceToStreet').show();

                $('#AlleyTurns').removeAttr('disabled');
                $('#divAlleyTurns').show();

                $('input[id^="AlleyWidth"]').removeAttr('disabled');

                var turns = $('#AlleyTurns').val();
                $('[id^="divAlleyWidth"]').hide();
                for (i = 0; i <= turns; i++) {
                    $('[id^="divAlleyWidth' + i + '"]').show();
                }
                $('[id^="divAlleyWidth' + 1 + '"]').show();
                break;

                // defaut                                                                  
            default:

                $('#StreetWidth').attr('disabled', 'disabled');
                $('#divStreetWidth').hide();

                $('#DistanceToStreet').attr('disabled', 'disabled');
                $('#divDistanceToStreet').hide();

                $('#AlleyTurns').attr('disabled', 'disabled');
                $('#divAlleyTurns').hide();

                $('input[id^="AlleyWidth"]').attr('disabled', 'disabled');
                $('[id^="divAlleyWidth"]').hide();
        }
    }).change();

    // AlleyTurns change
    $('#AlleyTurns').change(function () { if ($('#LocationCssClass').val() == "h-alley") { var turns = $(this).val(); if (turns > 6) { turns = 6; $(this).val('6') } if (turns <= 0) { turns = 1; $(this).val('1') } $('[id^="divAlleyWidth"]').hide(); for (i = 0; i <= turns; i++) { $('[id^="divAlleyWidth' + i + '"]').show(); } } }).change();

    // Convert Price from numbers to string
    $('#PriceProposed,#PaymentMethodId,#PaymentUnitId').change(function () { DocGiaRao(); });

    $('#PriceProposed').keyup(function () { DocGiaRao(); });
    if ($('#PriceProposed').length > 0)
        DocGiaRao();

    // Validate Address, check properties of current user only
    if ($('#AdsTypeCssClass').length > 0)
        $("#WardId,#StreetId,#AddressNumber,#AddressCorner,#AdsTypeCssClass").change(function () { validateUserPropertyAddress(); });
    else
        $("#StreetId,#AddressNumber,#AddressCorner").change(function () { validateUserPropertyAddress(); });

    // #endregion

    // Focus vào block Định giá khi user nhấn Định giá
    if ($('#SubmitEstimate').length > 0 && $('#SubmitEstimate').is(':checked')) { toggleEstimate(true); $('html, body').animate({ scrollTop: $('#block-estimate').offset().top }, 500); }

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

    // Giới hạn số lượng ký tự nhập vào
    if ($.fn.limiter) {
        if ($("#charsTitle").length != 0) { var elemTitle = $("#charsTitle"); $("#Title").limiter(120, elemTitle); }
        if ($("#charsContent").length != 0) { var elemContent = $("#charsContent"); $("#Content").limiter(1000, elemContent); }
        if ($("#charsNote").length != 0) { var elemNote = $("#charsNote"); $("#Note").limiter(500, elemNote); }
    }

    // FORM ACTION

    $('[name="submit.Estimate"]').click(function () {
        if (isNaN($('#PriceProposed').val()) || $('#PriceProposed').val() == '') $('#PriceProposed').val(-1);
    });

    $("form[method=get]").submit(function () {
        $('input[type=text], select, input[type=hidden]', this).each(function () { this.disabled = ($(this).val() == 'false' || $(this).val() == '') });
        $('input[type=checkbox]', this).each(function () { this.disabled = !($(this).is(':checked')) });
        $('button[name="submit.Filter"]').attr('disabled', 'disabled');
        $('input[name="__RequestVerificationToken"]').attr('disabled', 'disabled');
    });

    //$("form").submit(function () {
    //$('input[type="submit"][name!="submit.Estimate"], button[type="submit"][name!="submit.Estimate"]').attr('disabled', 'disabled').click(function () 
    //{ return false; });
    //});

    //12-09-2013
    $('input[type="submit"], button[type="submit"]').click(function () {
        var sDate = new Date(VNDateTimeToUTCDateTime($('.date-vip-start').val(), '/'));
        var eDate = new Date(VNDateTimeToUTCDateTime($('.date-vip-end').val(), '/'));
        if (sDate > eDate) {
            sDate.setDate(sDate.getDate() + 1);
            $('.date-vip-end').val(UTCDateTimeToVNDateTime(sDate, '/'));
        }
        var totalday = GetTotalDay(sDate, eDate);
        var vindex = parseInt($('#AdsTypeVIP').val());
        var IsApproved = $('#statusProperty').val();
        var oldVIP = $('#oldAdsTypeVip').val();
        var amount = 0;
        var oldAmount = 0;
        //Kiem tra so tien dang co va so tien tong dang tin vip
        if (vindex != 0 && vindex <= 3 && $('#AdsTypeVIP').is(':enabled')) {
            var amountCurent = parseInt($('#Amount').val());
            var oldTotalday = $('#oldPostingDates').val();// Tính lại số ngày cho trường hợp làm mới lại mà vẫn còn hạn tin VIP(NgayDangVip >? NgayHienTai => oldTotalday - (NgayHetHanVip - NgayHienTai) = NgayDaDangTinVip)
            if (IsApproved == 0 && oldVIP != 0) {
                var oldVIndex = Number(oldVIP - 1);
                var oldUnitprice = unitPriceArray[oldVIndex];
                oldAmount = Number(oldTotalday) * oldUnitprice;
            }

            vindex = Number(vindex - 1);
            var unitprice = unitPriceArray[vindex];
            amount = totalday * unitprice;
            if (oldTotalday < totalday && amount > Number(oldAmount + amountCurent)) {
                alert('Bạn không đủ tiền trong tài khoản để đăng tin VIP này. Vui lòng nạp tiền vào tài khoản đăng tin hoặc chọn đăng "tin thường" miễn phí.');
                return false;
            }
        }

        //$('#formUpdateId').submit()
        //if ($('#AdsVIPRequest').is(':checked')) {
        //    $('#formUpdateId').submit()
        //} else {
        //    if ($('#formUpdateId').valid()) {
        //        $("#CaptchatModal").modal('show')
        //        return false;
        //    }
        //}

        //Validate  AreaTotal
        var areaTotal = $("#AreaTotal", $(this).closest('form'));
        var areaTotalWidth = $("#AreaTotalWidth", $(this).closest('form'));
        var areaTotalLength = $("#AreaTotalLength", $(this).closest('form'));
        if (areaTotal.val().length > 0) {
            $(areaTotalWidth).removeAttr('required');
            $(areaTotalLength).removeAttr('required');
        } else {
            $(areaTotalWidth).attr('required', 'required');
            $(areaTotalLength).attr('required', 'required');
        }
    });
    $('#ModalUpdate').click(function () {
        $.ajax({
            type: "post",
            url: "/ajax/RealEstateForum/PostFrontEnd/ValidateCaptcha",
            data: {
                recaptcha_challenge_field: $('#recaptcha_challenge_field').val(), recaptcha_response_field: $('#recaptcha_response_field').val(),
                __RequestVerificationToken: antiForgeryToken
            },
            success: function (results) {
                if (results.status) {
                    $('#CaptchatModal').modal('hide');
                    $('#alertCaptcha').hide();
                    //submitform
                    $('#submitCaptcha').val('submit.Captcha');
                    $('#formUpdateId').submit();
                    Recaptcha.reload();
                } else {
                    $('#alertCaptcha').show();
                    Recaptcha.reload();
                }
            },
            error: function (request, status, error) {
                console.log("Có lỗi xảy ra!");
            }
        });
    });
    //End 12-09-2013

    // Load current User Contact information
    if ($("#ContactEmail").length > 0 || $("#Email").length > 0) getContactInfo();

    //// Find location on Map
    //$('#WardId,#StreetId,#AddressNumber,#AddressCorner').change(function () {
    //    if (typeof geocoder != 'undefined') {
    //        var province = $('#ProvinceId option:selected').text();
    //        var district = $('#DistrictId option:selected').text();
    //        var ward = $('#WardId option:selected').text();
    //        var street = $('#StreetId option:selected').text();
    //        var addressnumber = $('#AddressNumber').val();
    //        var addresscorner = $('#AddressCorner').val();

    //        if (province != '') $('#address').val(province);
    //        if (district != '' && district != '-- Vui lòng chọn --') $('#address').val(district + ', ' + $('#address').val());
    //        if (ward != '' && ward != '-- Vui lòng chọn --' && ward != '[Loading..]') $('#address').val(ward.split('-')[0] + ', ' + $('#address').val());
    //        if (street != '' && street != '-- Vui lòng chọn --') $('#address').val(street.split('-')[0] + ', ' + $('#address').val());
    //        if (addressnumber != '') $('#address').val(addressnumber + ', ' + $('#address').val());
    //        //console.log($('#address').val());
    //        // Find location on Map
    //        geocode();
    //    }
    //});
    $('.guide-help').click(function () {
        var data = $(this).attr('data-help');
        if (data == 'addresscorner') {
            $('#GuideModal .modal-title').text('Hướng dẫn ghi số nhà');
            $('#GuideModal .ghi-so-nha').show();
            $('#GuideModal .map-help').hide();
            $('#GuideModal .map-help').hide();
        }
        if (data == 'maphelp') {
            $('#GuideModal .modal-title').text('Hướng dẫn ghi số lần rẽ');
            $('#GuideModal .map-help').show();
            $('#GuideModal .ghi-so-nha').hide();
            $('#GuideModal .remainingvalue').hide();
        }
        if (data == 'remainingvalue') {
            $('#GuideModal .modal-title').text('Hướng dẫn đánh giá chất lượng còn lại của bđs');
            $('#GuideModal .map-help').hide();
            $('#GuideModal .ghi-so-nha').hide();
            $('#GuideModal .remainingvalue').show();
        }
        $('#GuideModal').modal('show');
    });

    //Move Map Item
    $($('.content-item-map')).insertBefore($('.after-map-item'));
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

function toggleEstimate(enable) {
    if (enable == true) {
        $('#lbAlleyWidth1').html('<span class="text-error">*</span> Lần rẽ thứ nhất (Hẻm đầu tiên) rộng:');
        $('#AlleyWidth1').attr('required', 'True');
        $('.estiametion-mode').show();
        $('#linkEstimate').parent().hide();
    }
    else {
        $('#lbAlleyWidth1').text('Hẻm rộng:');
        $('#AlleyWidth1').removeAttr('required');
        $('.estiametion-mode').hide();
    }
    // show/hide .estiametion-mode
    $('#LocationCssClass').change();
}

// Chuyển chế độ validate khi người dùng muốn Định giá BĐS
$("#linkEstimate").click(function () {
    alert("Để định giá bạn cần nhập thêm một số thông tin bắt buộc.");
    toggleEstimate(true);
    $(this).closest("form").validate().form();
    return false;
});

// Load Loại BĐS
function loadTypeCssClass() {
    $.ajax({
        type: "get",
        dataType: "",
        url: "/RealEstate.Admin/Home/GetPropertyTypesForJson",
        data: {
            adsTypeCssClass: $('#AdsTypeCssClass').val(),
            typeGroupCssClass: $('#TypeGroupCssClass').val()
        },
        success: function (response) {
            var selectedValue = $('#TypeId').val();
            $('#TypeId').empty().append("<option value=''>-- Vui lòng chọn --</option>");
            $.each(response.list, function (i, item) { $('#TypeId').append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
            $('#TypeId').val(selectedValue).change();
        },
        error: function (request, status, error) {
            $('#TypeId').empty().append("<option value=''>-- Vui lòng chọn --</option>");
            console.log(error);
        }
    });
}

// Kiểm tra trùng BĐS của user
function validateUserPropertyAddress() {

    var _id = parseInt($("#Id").html()); if (isNaN(_id)) _id = 0;
    var _provinceId = parseInt($("#ProvinceId").val());
    var _districtId = parseInt($("#DistrictId").val());
    var _wardId = parseInt($("#WardId").val());
    var _streetId = parseInt($("#StreetId").val());
    var _apartmentId = parseInt($("#ApartmentId").val()); if (isNaN(_apartmentId)) _apartmentId = 0;
    var _addressNumber = $("#AddressNumber").val();
    var _addressCorner = $("#AddressCorner").is(':visible') ? $("#AddressCorner").val() : "";
    var _apartmentNumber = $("#ApartmentNumber").val();
    var _adsTypeCssClass = $("#AdsTypeCssClass").val();

    if (_districtId > 0 &&
            (
                (_wardId > 0 && _streetId > 0 && _addressNumber != "" && _addressNumber != null)
                ||
                (_apartmentId > 0 && _apartmentNumber != "" && _apartmentNumber != null)
            )
        ) {
        $.ajax({
            type: "post",
            dataType: "",
            url: "/RealEstate.Admin/Home/AjaxUserValidateAddress",
            data: {
                id: _id,
                provinceId: _provinceId,
                districtId: _districtId,
                wardId: _wardId,
                streetId: _streetId,
                apartmentId: _apartmentId,
                addressNumber: _addressNumber,
                addressCorner: _addressCorner,
                apartmentNumber: _apartmentNumber,
                adsTypeCssClass: _adsTypeCssClass,
                __RequestVerificationToken: antiForgeryToken
            },
            success: function (response) {
                if (response.exists) {
                    if ((response.propertyEstimate || response.propertyExchange) && $('#formUpdateId').length > 0) {//If Form EditProperty
                        getSimilarInformation(_provinceId, _districtId, _wardId, _streetId, _apartmentId, _addressNumber, _addressCorner, _apartmentNumber);
                        return false;
                    }

                    var msg = '<div id="exists" class="message message-Warning">Bạn đã có BĐS: <a href="' + response.link + '">' + response.address + '</a> ' + response.info + '</div>';
                    if ($('#messages').length == 0) $('#layout-main').prepend('<div id="messages"><div class="zone zone-messages"></div></div>');
                    $('#messages .zone-messages').prepend(msg);

                    $('.primaryAction').attr('disabled', 'disabled').addClass('disabled');
                    if (confirm("BĐS: '" + response.address + "' đã có trong kho dữ liệu.\nBạn sẽ được tự động chuyển sang trang chỉnh sửa thông tin BĐS.")) {
                        $('#overlay').show();
                        location = response.link;
                    }
                }
                else {
                    $('#messages #exists').remove();
                    $('.primaryAction').removeAttr('disabled').removeClass('disabled');
                    if (response.existsInternal) {
                        getSimilarInformation(_provinceId, _districtId, _wardId, _streetId, _apartmentId, _addressNumber, _addressCorner, _apartmentNumber);
                    }
                }
            },
            error: function (request, status, error) {
            }
        });
    }
    return false;
}

// Load dữ liệu trong db để điền vào form nếu có
function getSimilarInformation(provinceId, districtId, wardId, streetId, apartmentId, addressNumber, addressCorner, apartmentNumber) {
    $.ajax({
        type: "post",
        dataType: "",
        url: "/ajax/RealEstate.FrontEnd/PropertySearch/LoadSameProperty",
        data: {
            provinceId: provinceId,
            districtId: districtId,
            wardId: wardId,
            streetId: streetId,
            apartmentId: apartmentId,
            addressNumber: addressNumber,
            addressCorner: addressCorner,
            apartmentNumber: apartmentNumber,
            __RequestVerificationToken: antiForgeryToken
        },
        success: function (response) {
            if (response.success == true) {
                var autoFill = false;

                // Type
                if (response.TypeId > 0) if ($("#TypeId").length > 0)/* if ($("#TypeId").val() == '')*/ { $('#TypeId').val(response.TypeId).change(); autoFill = true; }

                // LegalStatus, Direction, Location
                if (response.LegalStatusId > 0) if ($("#LegalStatusId").length > 0) if ($("#LegalStatusId").val() == '') { $("#LegalStatusId").val(response.LegalStatusId).change(); autoFill = true; }
                if (response.DirectionId > 0) if ($("#DirectionId").length > 0) if ($("#DirectionId").val() == '') { $("#DirectionId").val(response.DirectionId).change(); autoFill = true; }
                if (response.LocationCssClass != '') if ($("#LocationCssClass").length > 0) if ($("#LocationCssClass").val() == '') { $("#LocationCssClass").val(response.LocationCssClass).change(); autoFill = true; }

                // Alley
                if (response.DistanceToStreet > 0) if ($("#DistanceToStreet").length > 0) if ($("#DistanceToStreet").val() == '') { $("#DistanceToStreet").val(response.DistanceToStreet); autoFill = true; }
                if (response.AlleyTurns > 0) if ($("#AlleyTurns").length > 0) if ($("#AlleyTurns").val() == '') { $("#AlleyTurns").val(response.AlleyTurns).change(); autoFill = true; }
                if (response.AlleyWidth1 > 0) if ($("#AlleyWidth1").length > 0) if ($("#AlleyWidth1").val() == '') { $("#AlleyWidth1").val(response.AlleyWidth1); autoFill = true; }
                if (response.AlleyWidth2 > 0) if ($("#AlleyWidth2").length > 0) if ($("#AlleyWidth2").val() == '') { $("#AlleyWidth2").val(response.AlleyWidth2); autoFill = true; }
                if (response.AlleyWidth3 > 0) if ($("#AlleyWidth3").length > 0) if ($("#AlleyWidth3").val() == '') { $("#AlleyWidth3").val(response.AlleyWidth3); autoFill = true; }
                if (response.AlleyWidth4 > 0) if ($("#AlleyWidth4").length > 0) if ($("#AlleyWidth4").val() == '') { $("#AlleyWidth4").val(response.AlleyWidth4); autoFill = true; }
                if (response.AlleyWidth5 > 0) if ($("#AlleyWidth5").length > 0) if ($("#AlleyWidth5").val() == '') { $("#AlleyWidth5").val(response.AlleyWidth5); autoFill = true; }
                if (response.AlleyWidth6 > 0) if ($("#AlleyWidth6").length > 0) if ($("#AlleyWidth6").val() == '') { $("#AlleyWidth6").val(response.AlleyWidth6); autoFill = true; }
                if (response.StreetWidth > 0) if ($("#StreetWidth").length > 0) if ($("#StreetWidth").val() == '') { $("#StreetWidth").val(response.StreetWidth); autoFill = true; }

                // AreaTotal
                if (response.AreaTotal > 0) if ($("#AreaTotal").length > 0) if ($("#AreaTotal").val() == '') { $("#AreaTotal").val(response.AreaTotal); autoFill = true; }
                if (response.AreaTotalWidth > 0) if ($("#AreaTotalWidth").length > 0) if ($("#AreaTotalWidth").val() == '') { $("#AreaTotalWidth").val(response.AreaTotalWidth); autoFill = true; }
                if (response.AreaTotalLength > 0) if ($("#AreaTotalLength").length > 0) if ($("#AreaTotalLength").val() == '') { $("#AreaTotalLength").val(response.AreaTotalLength); autoFill = true; }
                if (response.AreaTotalBackWidth > 0) if ($("#AreaTotalBackWidth").length > 0) if ($("#AreaTotalBackWidth").val() == '') { $("#AreaTotalBackWidth").val(response.AreaTotalBackWidth); autoFill = true; }

                // AreaLegal
                if (response.AreaLegal > 0) if ($("#AreaLegal").length > 0) if ($("#AreaLegal").val() == '') { $("#AreaLegal").val(response.AreaLegal); autoFill = true; }
                if (response.AreaLegalWidth > 0) if ($("#AreaLegalWidth").length > 0) if ($("#AreaLegalWidth").val() == '') { $("#AreaLegalWidth").val(response.AreaLegalWidth); autoFill = true; }
                if (response.AreaLegalLength > 0) if ($("#AreaLegalLength").length > 0) if ($("#AreaLegalLength").val() == '') { $("#AreaLegalLength").val(response.AreaLegalLength); autoFill = true; }
                if (response.AreaLegalBackWidth > 0) if ($("#AreaLegalBackWidth").length > 0) if ($("#AreaLegalBackWidth").val() == '') { $("#AreaLegalBackWidth").val(response.AreaLegalBackWidth); autoFill = true; }
                if (response.AreaIlegalRecognized > 0) if ($("#AreaIlegalRecognized").length > 0) if ($("#AreaIlegalRecognized").val() == '') { $("#AreaIlegalRecognized").val(response.AreaIlegalRecognized); autoFill = true; }

                // AreaUsable
                if (response.AreaUsable > 0) if ($("#AreaUsable").length > 0) if ($("#AreaUsable").val() == '') { $("#AreaUsable").val(response.AreaUsable); autoFill = true; }

                // AreaResidential
                if (response.AreaResidential > 0) if ($("#AreaResidential").length > 0) if ($("#AreaResidential").val() == '') { $("#AreaResidential").val(response.AreaResidential); autoFill = true; }

                // Construction
                if (response.AreaConstruction > 0) if ($("#AreaConstruction").length > 0) if ($("#AreaConstruction").val() == '') { $("#AreaConstruction").val(response.AreaConstruction); autoFill = true; }
                if (response.AreaConstructionFloor > 0) if ($("#AreaConstructionFloor").length > 0) if ($("#AreaConstructionFloor").val() == '') { $("#AreaConstructionFloor").val(response.AreaConstructionFloor); autoFill = true; }

                // TypeConstruction
                if (response.TypeConstructionId > 0) {
                    if ($("#TypeConstructionId").length > 0) {
                        if ($("#TypeConstructionId").val() == '') {
                            $("#TypeConstructionId").data("autofill", response.TypeConstructionId).val(response.TypeConstructionId).change();
                            autoFill = true;
                        }
                    }
                }

                if (response.Floors > 0) if ($("#Floors").length > 0) if ($("#Floors").val() == '') { $("#Floors").val(response.Floors); autoFill = true; }
                if (response.FloorsCount != 0) if ($("#FloorsCount").length > 0) if ($("#FloorsCount").val() == '' || $("#FloorsCount").val() == '0') { $("#FloorsCount").val(response.FloorsCount).change(); autoFill = true; }
                if (response.Bedrooms > 0) if ($("#Bedrooms").length > 0) if ($("#Bedrooms").val() == '') { $("#Bedrooms").val(response.Bedrooms); autoFill = true; }
                if (response.Bathrooms > 0) if ($("#Bathrooms").length > 0) if ($("#Bathrooms").val() == '') { $("#Bathrooms").val(response.Bathrooms); autoFill = true; }

                if (response.InteriorId > 0) if ($("#InteriorId").length > 0) if ($("#InteriorId").val() == '') { $("#InteriorId").val(response.InteriorId).change(); autoFill = true; }
                if (response.RemainingValue > 0) if ($("#RemainingValue").length > 0) if ($("#RemainingValue").val() == '') { $("#RemainingValue").val(response.RemainingValue); autoFill = true; }

                if (response.HaveBasement == true) { $("#HaveBasement").prop("checked", true); autoFill = true; }
                if (response.HaveMezzanine == true) { $("#HaveMezzanine").prop("checked", true); autoFill = true; }
                if (response.HaveTerrace == true) { $("#HaveTerrace").prop("checked", true); autoFill = true; }
                if (response.HaveGarage == true) { $("#HaveGarage").prop("checked", true); autoFill = true; }
                if (response.HaveElevator == true) { $("#HaveElevator").prop("checked", true); autoFill = true; }
                if (response.HaveSwimmingPool == true) { $("#HaveSwimmingPool").prop("checked", true); autoFill = true; }

                // Apartment
                if (response.ApartmentFloors > 0) if ($("#ApartmentFloors").length > 0) if ($("#ApartmentFloors").val() == '') { $("#ApartmentFloors").val(response.ApartmentFloors); autoFill = true; }
                if (response.ApartmentElevators > 0) if ($("#ApartmentElevators").length > 0) if ($("#ApartmentElevators").val() == '') { $("#ApartmentElevators").val(response.ApartmentElevators); autoFill = true; }
                if (response.ApartmentBasements > 0) if ($("#ApartmentBasements").length > 0) if ($("#ApartmentBasements").val() == '') { $("#ApartmentBasements").val(response.ApartmentBasements); autoFill = true; }

                // Advantages
                // TODO

                // DisAdvantages
                // TODO

                // ApartmentAdvantages
                // TODO

                // ApartmentInteriorAdvantages
                // TODO

                if (autoFill == true) {
                    alert('Hệ thống đã tự động cập nhật thông tin tham khảo về BĐS này. Bạn có thể sửa lại cho chính xác.');

                    var areaLegal = $("#AreaLegal");
                    if (areaLegal.val() == null || areaLegal.val() == 0) {
                        var areaLegalWidth = $("#AreaLegalWidth");
                        var areaLegalBackWidth = $("#AreaLegalBackWidth");
                        var areaLegalLength = $("#AreaLegalLength");

                        //Fill textbox
                        var areaNumber = ((Number(areaLegalWidth.val()) + Number(areaLegalBackWidth.val())) / 2) * areaLegalLength.val();
                        if (areaLegalBackWidth.val() == null || areaLegalBackWidth.val() == 0)
                            areaNumber = Number(areaLegalWidth.val()) * Number(areaLegalLength.val());

                        areaLegal.val(areaNumber);

                    }
                }

            }
        },
        error: function (request, status, error) {
        }
    });
}

//Character Limit Counter
(function ($) { $.fn.extend({ limiter: function (limit, elem) { $(this).on("keyup focus", function () { setCount(this, elem); }); function setCount(src, elem) { var chars = src.value.length; if (chars > limit) { src.value = src.value.substr(0, limit); chars = limit; } elem.html(limit - chars); } setCount($(this)[0], elem); } }); })(jQuery);

// Load calendar
$(function () {
    if ($.fn.datepicker) {
        $('.date-box').datepicker({
            dateFormat: "dd/mm/yy",
            changeMonth: true,
            changeYear: true,
            numberOfMonths: 1,
            onSelect: function (selectedDate) {
                id = $(this).attr('id');
                if (id.indexOf('From') >= 0)
                    $('#' + $(this).attr('id').replace('From', 'To')).datepicker('option', 'minDate', selectedDate);
                else if (id.indexOf('To') >= 0)
                    $('#' + $(this).attr('id').replace('To', 'From')).datepicker("option", "maxDate", selectedDate);
            }
        });
        $('.date-vip').datepicker({
            dateFormat: 'dd/mm/yy',
            changeMonth: true,
            minDate: new Date()
        });

        $('.date-vip-start, .date-vip-end').change(function () {
            ResetDateVip(2);
        });
    }
    ResetDateVip(3);
});
$('#AdsTypeVIP').change(function () {
    ResetDateVip(1);
});

$('#DistrictId').change(function () {
    ResetDateVip(3);
});

$('#formUpdateId').submit(function (e) {
    //var submit = ResetDateVip(4);
    //alert(submit);
    //alert(submit != undefined);
    //alert(submit == false);
    //alert(submit != undefined && submit == false);
    //if (submit != undefined && submit == false) {
    //    return false;
    //}
    //else $('#formUpdateId').submit();
});
// URL Params
function getURLParameter(name) { return decodeURI((RegExp(name + '=' + '(.+?)(&|$)').exec(location.search) || [, null])[1]); }

function removeByValue(arr, val) { for (var i = 0; i < arr.length; i++) { if (arr[i] == val) { arr.splice(i, 1); break; } } }

/*
* Convert money from numbers to words in VND
*
*/
// #region Convert money from numbers to words in VND

function DocGiaRao() {
    var value = '';
    var number = parseFloat($('#PriceProposed').val().replace(/,/g, ''));
    if (number != '' && number != '-1' && !isNaN(number)) {
        var unitValue = $('#PaymentMethodId option:selected').val();
        var unitText = $('#PaymentMethodId option:selected').text();

        switch (unitValue) {
            case '56':
                number = number * 1000000000; // Tỷ
                unitText = "đồng";
                break;
            case '57':
                number = number * 1000000; // Triệu
                unitText = "đồng";
                break;
            case '58':
                number = number * 1000; // Nghìn
                unitText = "đồng";
                break;
            case '60':
                number = number; // USD
                unitText = "USD";
                break;
            case '134122':
                number = number * 1000000; // Triệu USD
                unitText = "USD";
                break;
        }

        // Fix Floating Point problem in javascript
        number = parseFloat(number) + parseFloat(0.009);
        numberStr = number.toString();
        number = numberStr.substring(0, numberStr.indexOf('.'));

        value = DocTienBangChu(number) + " " + unitText + " / " + $('#PaymentUnitId option:selected').text();

        $('#PriceInWords').html(value);
    }
    else {
        $('#PriceInWords').html('');
    }
}

//1. Hàm đọc số có ba chữ số;
function DocSo3ChuSo(baso) {
    var tram = parseInt(baso / 100); var chuc = parseInt((baso % 100) / 10); var donvi = baso % 10; var KetQua = "";
    if (tram == 0 && chuc == 0 && donvi == 0) return "";
    if (tram != 0) { KetQua += ChuSo[tram] + " trăm "; if ((chuc == 0) && (donvi != 0)) KetQua += " linh "; }
    if ((chuc != 0) && (chuc != 1)) { KetQua += ChuSo[chuc] + " mươi"; if ((chuc == 0) && (donvi != 0)) KetQua = KetQua + " linh "; }
    if (chuc == 1) KetQua += " mười ";
    switch (donvi) {
        case 1: if ((chuc != 0) && (chuc != 1)) { KetQua += " mốt "; } else { KetQua += ChuSo[donvi]; } break;
        case 5: if (chuc == 0) { KetQua += ChuSo[donvi]; } else { KetQua += " lăm "; } break;
        default: if (donvi != 0) { KetQua += ChuSo[donvi]; } break;
    }
    return KetQua;
}

//2. Hàm đọc số thành chữ (Sử dụng hàm đọc số có ba chữ số)

function DocTienBangChu(SoTien) {
    var lan = 0; var i = 0; var so = 0; var KetQua = ""; var tmp = ""; var ViTri = new Array();
    if (SoTien < 0) return "Số tiền âm !"; if (SoTien == 0) return "Không đồng !"; if (SoTien > 0) { so = SoTien; } else { so = -SoTien; }
    if (SoTien > 8999999999999999) { return "Số quá lớn!"; }
    ViTri[5] = Math.floor(so / 1000000000000000);
    if (isNaN(ViTri[5])) ViTri[5] = "0"; so = so - parseFloat(ViTri[5].toString()) * 1000000000000000;
    ViTri[4] = Math.floor(so / 1000000000000);
    if (isNaN(ViTri[4])) ViTri[4] = "0"; so = so - parseFloat(ViTri[4].toString()) * 1000000000000;
    ViTri[3] = Math.floor(so / 1000000000);
    if (isNaN(ViTri[3])) ViTri[3] = "0"; so = so - parseFloat(ViTri[3].toString()) * 1000000000;
    ViTri[2] = parseInt(so / 1000000);
    if (isNaN(ViTri[2])) ViTri[2] = "0";
    ViTri[1] = parseInt((so % 1000000) / 1000);
    if (isNaN(ViTri[1])) ViTri[1] = "0";
    ViTri[0] = parseInt(so % 1000);
    if (isNaN(ViTri[0])) ViTri[0] = "0";
    if (ViTri[5] > 0) { lan = 5; } else if (ViTri[4] > 0) { lan = 4; } else if (ViTri[3] > 0) { lan = 3; } else if (ViTri[2] > 0) { lan = 2; } else if (ViTri[1] > 0) { lan = 1; } else { lan = 0; };
    for (i = lan; i >= 0; i--) { tmp = DocSo3ChuSo(ViTri[i]); KetQua += tmp; if (ViTri[i] > 0) KetQua += Tien[i]; if ((i > 0) && (tmp.length > 0)) KetQua += ',';  /*&& (!string.IsNullOrEmpty(tmp))*/ };
    if (KetQua.substring(KetQua.length - 1) == ',') { KetQua = KetQua.substring(0, KetQua.length - 1); } KetQua = KetQua.substring(1, 2).toUpperCase() + KetQua.substring(2);

    // Đọc phần thập phân
    //strPhanThapPhan = SoTien.toString();
    //floatingPointIndex = strPhanThapPhan.indexOf('.');
    //var PhanThapPhan = "";
    //if (floatingPointIndex > -1) PhanThapPhan = "phẩy " + DocTienBangChu(strPhanThapPhan.substring(floatingPointIndex + 1));

    return KetQua; //.substring(0, 1);//.toUpperCase();// + KetQua.substring(1);
}

// #endregion

function checkIsValidVip() {
    var districtId = $('#DistrictId').val();
    var adsTypeCssClass = $('#AdsTypeCssClass').val();

    var districArr = [313, 315, 317, 322, 323, 328, 329, 326, 330];//1,3,5,10,11,PN,TB,BThanh,TPhu
    var isValid = false;
    if (districtId != null && districtId != '' && adsTypeCssClass == 'ad-selling' && (districArr.indexOf(parseInt(districtId)) > -1)) {
        isValid = true;
    }
    return isValid;
}
function ResetDateVip(type) {//1. change selecte 2. change date  3. ready/Change district 4.submit form
    try {

        //#region INIT
        if ($('.date-vip-start').val().length == 0) {
            $('.date-vip-start').val(UTCDateTimeToVNDateTime(new Date(), '/'));
        }
        if ($('.date-vip-end').val().length == 0) {
            var tmpdate = new Date();
            tmpdate.setMonth(tmpdate.getMonth() + 1);
            $('.date-vip-end').val(UTCDateTimeToVNDateTime(tmpdate, '/'));
        }
        var sDate = new Date(VNDateTimeToUTCDateTime($('.date-vip-start').val(), '/'));
        var eDate = new Date(VNDateTimeToUTCDateTime($('.date-vip-end').val(), '/'));
        if ($('.date-vip-end').attr('disabled') != undefined) {
            //var newDate = new Date(sDate);
            //newDate.setMonth(newDate.getMonth() + 3);
            //$('.date-vip-end').val(UTCDateTimeToVNDateTime(newDate, '/'));
        }
        //#endregion

        var newMaxDate = new Date(sDate);
        var newMinDate = new Date(sDate);

        newMaxDate.setMonth(newMaxDate.getMonth() + 6);
        var newMaxStartDate = new Date(eDate);
        var isValid = checkIsValidVip();

        if (type == 1 || type == 3) {
            var adsTypeVip = parseInt($('#AdsTypeVIP').val());
            var newDate = new Date(sDate);
            //So sánh số tiền hiện có với giá tin vip xem có đủ đăng dc mấy ngày
            var amountCurent = parseInt($('#Amount').val());
            var unitprice = 0;

            switch (adsTypeVip) {
                case 0:
                    if (isValid) {
                        newDate.setDate(newDate.getDate() + 15);//Tin thường có tính phí
                        unitprice = unitPriceArray[0]
                    }
                    else
                        newDate.setDate(newDate.getDate() + 30);// Tin thường không tính phí
                    break;
                case 1:
                    newDate.setDate(newDate.getDate() + 10);
                    unitprice = unitPriceArray[1];
                    break;
                case 2:
                    newDate.setDate(newDate.getDate() + 5);
                    unitprice = unitPriceArray[2];
                    break;
                case 3:
                    newDate.setDate(newDate.getDate() + 3);
                    unitprice = unitPriceArray[3];
                    break;
            }            

            var totalday = GetTotalDay(sDate, newDate);

            //if (((adsTypeVip == 0 && isValid) || adsTypeVip != 0) && amountCurent > 0 && amountCurent < unitprice * totalday) {
            //    var temp = Number((amountCurent - (amountCurent % unitprice)) / unitprice);
            //    newDate = new Date(sDate);
            //    newDate.setDate(newDate.getDate() + temp);
            //}

            if (type == 1)
                $('.date-vip-end').val(UTCDateTimeToVNDateTime(newDate, '/'));
            $(".date-vip-end").datepicker("option", "minDate", newDate);
        }

        newMinDate.setDate(newMinDate.getDate() + 1);
        newMaxStartDate.setSeconds(newMaxStartDate.getSeconds() + 86399);//end date
        newMaxStartDate.setDate(newMaxStartDate.getDate() - 1);

        if (newMaxStartDate < new Date()) {
            newMaxStartDate = new Date();
            eDate = newMinDate;
            $('.date-vip-end').val(UTCDateTimeToVNDateTime(eDate, '/'));
        }

        $(".date-vip-end").datepicker("option", "maxDate", newMaxDate);
        $(".date-vip-start").datepicker("option", "maxDate", new Date());

        var _alert = type != 3 ? true : false;

        return GetAbsAmount(type, _alert);
    } catch (ex) {
        //alert(ex.message);
        if (parseInt($('#AdsTypeVIP').val()) == 0) {
            $('.paymentAmount').html('Miễn phí');
        }
        console.log(ex);
    }
}
function VNDateTimeToUTCDateTime(vnDatetime, separatorChar) {
    try {
        var strDate = vnDatetime.toString();
        var aDate = strDate.split(separatorChar);
        var retDate = new Date(aDate[2] + '/' + aDate[1] + '/' + aDate[0]).toDateString();
        return retDate;
    } catch (ex) {
        return new Date();
    }
}
function UTCDateTimeToVNDateTime(utcDatetime, separator) {
    var strdate = '';
    try {
        strdate = new Date(utcDatetime);
    } catch (ex) {
        strdate = new Date();
    }
    var day = strdate.getDate() < 9 ? '0' + (strdate.getDate()) : strdate.getDate();
    var month = strdate.getMonth() < 9 ? "0" + (strdate.getMonth() + 1) : (strdate.getMonth() + 1);
    var retDate = day + separator + month + separator + strdate.getFullYear();
    return retDate;
}

function GetAbsAmount(type, _alert) {//1. change selecte 2. change date  3. ready

    var sDate = new Date(VNDateTimeToUTCDateTime($('.date-vip-start').val(), '/'));
    var eDate = new Date(VNDateTimeToUTCDateTime($('.date-vip-end').val(), '/'));
    if (sDate > eDate) {
        sDate.setDate(sDate.getDate() + 1);
        $('.date-vip-end').val(UTCDateTimeToVNDateTime(sDate, '/'));
    }
    var totalday = GetTotalDay(sDate, eDate);
    var vindex = parseInt($('#AdsTypeVIP').val());
    var IsApproved = $('#statusProperty').val();
    var oldVIP = $('#oldAdsTypeVip').val();
    var amount = 0;
    var oldAmount = 0;
    //Kiem tra so tien dang co va so tien tong dang tin vip
    var isValid = checkIsValidVip();

    if (isValid) {
        var amountCurent = parseInt($('#Amount').val());
        if (IsApproved == 0) {//&& oldVIP != 0
            var oldTotalday = $('#oldPostingDates').val();
            var oldVIndex = Number(oldVIP);
            var oldUnitprice = unitPriceArray[oldVIndex];
            oldAmount = oldTotalday >= Date() ? Number(oldTotalday) * oldUnitprice : 0;

            //alert('oldTotalday: ' + oldTotalday + ' -  oldVIndex: '+ oldVIndex +' - oldUnitprice' + oldUnitprice);

            var unitprice = unitPriceArray[vindex];
            amount = totalday * unitprice;
            //alert('amount: ' + amount + ' - totalday:' + totalday + ' - vindex:' + vindex + ' - unitprice:' + unitprice);
            
            $('.paymentAmount').html('(' + amount + ') ' + DocTienBangChu(amount));
            //alert('amount: ' + amount + ' > ' + '(oldAmount: ' + oldAmount + ' + amountCurent: ' + amountCurent + ')' + 'oldVIP: ' + oldVIP + ' != ' + $('#AdsTypeVIP').val() + ' || ' + 'oldTotalday: ' + oldTotalday + ' != ' + totalday);
            if (amount > Number(oldAmount + amountCurent)) {
                if (_alert) {
                    if (isValid)
                        alert('Khu vực có thu phí:\n- Tin thường 1000 vnd/ngày \n- Vip 1: 5000 vnd/ngày \n- Vip 2: 3000 vnd/ngày \n- Vip 3: 1500 vnd/ngày \n- Vui lòng nạp tiền để đăng tin này');
                    else
                        alert('Số tiền không đủ để đăng tin.');

                    if (type == 4)
                        return false;

                    return false;
                }
            }
        }
    } else {
        $('.paymentAmount').html('Miễn phí');
    }
}

function GetTotalDay(sday, eday) {
    var d = eday - sday;
    d = d / 24;
    d = d / 60;
    d = d / 60;
    d = d / 1000;
    return d;
}

$('#AreaLegalWidth').change(function () {
    var areaLegalBackWidth = $("#AreaLegalBackWidth", $(this).closest('form'));
    areaLegalBackWidth.val($(this).val());
});

//Auto Calculate
$('#AreaLegalWidth, #AreaLegalLength, #AreaLegalBackWidth').change(function () {
    //var flagAutoCal = $("#FlagAutoCal", $(this).closest('form'));
    //if (flagAutoCal == 'false') return ;

    var areaLegalWidth = $("#AreaLegalWidth", $(this).closest('form'));
    var areaLegalBackWidth = $("#AreaLegalBackWidth", $(this).closest('form'));
    var areaLegalLength = $("#AreaLegalLength", $(this).closest('form'));
    var areaLegal = $("#AreaLegal", $(this).closest('form'));

    //Fill textbox
    var areaNumber = ((Number(areaLegalWidth.val()) + Number(areaLegalBackWidth.val())) / 2) * areaLegalLength.val();
    if (areaLegalBackWidth.val() == null || areaLegalBackWidth.val() == 0)
        areaNumber = Number(areaLegalWidth.val()) * Number(areaLegalLength.val());
    //Fill textbox
    areaLegal.val(areaNumber).change();

    var typeId = $("#TypeId", $(this).closest('form')).val();
    if (typeId != 231 && typeId != '' && typeId != null) {
        var areaConstruction = $("#AreaConstruction", $(this).closest('form'));
        //Fill textbox
        areaConstruction.val(areaNumber);

        var area = AreaContructionChange($(areaConstruction));
        if (area > 0) {
            var areaConstructionFloor = $("#AreaConstructionFloor", $(this).closest('form'));
            //Fill textbox
            areaConstructionFloor.val(area);
        }
    }
});

$('#Floors').change(function () {
    //var flagAutoCal = $("#FlagAutoCal", $(this).closest('form'));
    //if (flagAutoCal == 'false') return;

    var floors = Number($(this).val());
    var areaConstruction = $("#AreaConstruction", $(this).closest('form'));
    var areaConstructionFloor = $("#AreaConstructionFloor", $(this).closest('form'));
    if (floors > 10) {

        var haveBasement = $("#HaveBasement", $(this).closest('form'));
        var haveMezzanine = $("#HaveMezzanine", $(this).closest('form'));
        var haveTerrace = $("#HaveTerrace", $(this).closest('form'));

        var tempCount = 0;
        if (haveBasement.is(':checked'))
            tempCount += 1;

        if (haveMezzanine.is(':checked'))
            tempCount += 1;

        if (haveTerrace.is(':checked'))
            tempCount += 1;

        var temp = (floors + 1 + Number(tempCount)) * areaConstruction.val();
        //Fill textbox
        areaConstructionFloor.val(temp);
    }
});

$('#HaveBasement, #HaveMezzanine, #HaveTerrace').change(function () {
    //var flagAutoCal = $("#FlagAutoCal", $(this).closest('form'));
    //if (flagAutoCal == 'false') return;

    var areaConstructionFloor = $("#AreaConstructionFloor", $(this).closest('form'));
    var area = AreaContructionChange($(this));

    if (area > 0) {
        //Fill textbox
        areaConstructionFloor.val(area);
    }
});

function AreaContructionChange($that) {
    var haveBasement = $("#HaveBasement", $($that).closest('form'));
    var haveMezzanine = $("#HaveMezzanine", $($that).closest('form'));
    var haveTerrace = $("#HaveTerrace", $($that).closest('form'));

    var tempCount = 0;
    if (haveBasement.is(':checked'))
        tempCount += 1;

    if (haveMezzanine.is(':checked'))
        tempCount += 1;

    if (haveTerrace.is(':checked'))
        tempCount += 1;

    var floorCount = Number($("#FloorsCount", $($that).closest('form')).val());
    var areaConstruction = $("#AreaConstruction", $($that).closest('form'));

    if (floorCount == -1) {
        floorCount = Number($('#Floors').val());
    }
    var temp = (floorCount + 1 + Number(tempCount)) * areaConstruction.val();

    return temp;
}
