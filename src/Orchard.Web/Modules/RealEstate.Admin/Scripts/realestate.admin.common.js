
// blink AdsRequest
window.setInterval("$('.icon-coin').toggleClass('icon-blink');", 500);
//window.setInterval("$('.icon-coin-24').toggleClass('icon-blink');", 800);

window.mobileAndTabletcheck = function () {
    var check = false;
    (function (a) { if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino|android|ipad|playbook|silk/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true })(navigator.userAgent || navigator.vendor || window.opera);
    return check;
}

function initTooltip() {
    if ($.fn.tooltip) {
        if ($('[data-toggle="tooltip"]').length > 0) {
            $(document).tooltip({
                items: '[data-toggle="tooltip"]',
                content: function () {
                    var element = $(this);
                    var elementContent = element.siblings('[data-tooltip-content]');
                    if (elementContent.length > 0) {
                        return elementContent.html();
                    }
                    if (element.is("[title]")) {
                        return element.attr("title");
                    }
                }
            });
        }
    }
}

$(function () {

    // #region Toggle Hide / Show

    $('legend .icon-expand').click(function () {
        $(this).toggleClass('icon-collapse');
        $(this).parent().siblings().toggle();
    });

    // Debug Panel
    $('.ui-growl-image-info').click(function () {
        $(this).siblings('.ui-growl-message').toggle('slow');
    });

    // Init tipTip
    //if ($.fn.tipTip) $(".tlp").each(function () { $(this).tipTip({ maxWidth: "auto" }); }).on('click', function () { return false; });

    initTooltip();


    // Table Hover - Table Select
    $('table.items tbody tr').hover(function () { $(this).addClass('highlight'); }, function () { $(this).removeClass('highlight'); });
    $('table.items tbody tr td input[type=checkbox]').change(function () { $(this).closest('tr').toggleClass('selected', $(this).is(':checked')); });
    $('.selectAll').change(function () { $('input:checkbox[name^=' + this.value + ']').attr('checked', this.checked).change(); });

    // Advance Search
    if ($.cookie('advance-search-expand') == 'true') {
        $('.advance-search-trigger').find('.ui-icon').addClass('icon-collapse');
        $('#tblAdvanceSearch').show();
    } else {
        $('#tblAdvanceSearch').hide();
    }
    $('.advance-search-trigger').click(function () {
        $(this).find('.ui-icon').toggleClass('icon-collapse');
        $('#tblAdvanceSearch').toggle('fast');
        $.cookie('advance-search-expand', !($.cookie('advance-search-expand') == 'true'));
        return false;
    });

    function toggleBackgroundImage() {
        $('html, body, #layout-content, #footer').toggleClass('disable-bg-img');
    }

    // Expand Menu

    $('#branding').append('<div id="menu-expand-collapse" class="menu-collapse"></div>');
    $('#menu-expand-collapse').on('click', function () {
        if ($('#menu').is(":visible")) {
            $('#menu').hide('slow');
            $('html, body, #layout-content, #footer').addClass('disable-bg-img');
            $('#main').animate({ marginLeft: "20px" }, 500);
            $(this).addClass('menu-expand');
            $.cookie('menu-expand', true);
        } else {
            $('#menu').show('slow');
            $('html, body, #layout-content, #footer').removeClass('disable-bg-img');
            $('#main').animate({ marginLeft: "260px" }, 500);
            $(this).removeClass('menu-expand');
            $.cookie('menu-expand', false);
        }
    });

    if ($.cookie('menu-expand') == 'true') {
        $('#menu-expand-collapse').click();
    }

    // #endregion

    // #region Create / Edit Property

    $('#StatusCssClass').change(function () {
        if ($(this).val() == 'st-sold') {
            //$('#IsSoldByGroup').prop('checked', confirm('BĐS được bán bởi Group?'));
            $('#dialogIsSoldByGroup').modal('show').one('click', 'button', function () {
                $('#IsSoldByGroup').prop('checked', $(this).val() == 'true');
            });
        }
    })

    // Province change - If Hà Nội -> show AddressCorner
    function toggleAddressCorner() {
        if ($('#ProvinceId', $('#AddressCorner').closest('form')).children('option:selected').text() == 'Hà Nội') {
            $('#AddressCorner').closest('.control-group').show();
        } else {
            $('#AddressCorner').closest('.control-group').hide();
        }
    };

    toggleAddressCorner();
    $('#ProvinceId', $('#AddressCorner').closest('form')).change(function () {
        toggleAddressCorner();
    });

    // #region Toggle OtherWardName and OtherStreetName

    $('#ChkOtherDistrictName').change(function () {
        if ($(this).is(':checked')) {
            $('#DistrictId').val('').change().siblings('.combobox-container').hide();
            $('#OtherDistrictName').show();

            $('#ChkOtherWardName').attr('checked', true).change();
            $('#ChkOtherStreetName').attr('checked', true).change();
            $('#ChkOtherProjectName').attr('checked', true).change();
        } else {
            $('#DistrictId').val('').change().siblings('.combobox-container').show();
            $('#OtherDistrictName').hide();

            $('#ChkOtherWardName').attr('checked', false).change();
            $('#ChkOtherStreetName').attr('checked', false).change();
            $('#ChkOtherProjectName').attr('checked', false).change();
        }
        //$(this).closest("form").validate().element($('#DistrictId'));
        //$(this).closest("form").validate().element($('#OtherDistrictName'));
    });
    $('#ChkOtherWardName').change(function () {
        if ($(this).is(':checked')) {
            $('#WardId').val('').siblings('.combobox-container').hide();
            $('#OtherWardName').show();
        } else {
            $('#WardId').val('').siblings('.combobox-container').show();
            $('#OtherWardName').hide();
        }
        //$(this).closest("form").validate().element($('#WardId'));
        //$(this).closest("form").validate().element($('#OtherWardName'));
    });
    $('#ChkOtherStreetName').change(function () {
        if ($(this).is(':checked')) {
            $('#StreetId').val('').siblings('.combobox-container').hide();
            $('#OtherStreetName').show();
        } else {
            $('#StreetId').val('').siblings('.combobox-container').show();
            $('#OtherStreetName').hide();
        }
        //$(this).closest("form").validate().element($('#StreetId'));
        //$(this).closest("form").validate().element($('#OtherStreetName'));
    });
    $('#ChkOtherProjectName').change(function () {
        var slcApartmentId = $("#ApartmentId", $(this).closest('form'));
        var slcApartmentIds = $("#ApartmentIds", $(this).closest('form'));
        if ($(this).is(':checked')) {
            $('#ApartmentId').val('').siblings('.combobox-container').hide();
            slcApartmentId.empty();
            if (slcApartmentIds.length > 0) {
                slcApartmentIds.siblings('.btn-group.form-control').hide();
                slcApartmentIds.multiselect('rebuild');
            }
            $('#OtherProjectName').show();
        } else {
            $('#ApartmentId').val('').siblings('.combobox-container').show();
            if (slcApartmentIds.length > 0) {
                slcApartmentIds.siblings('.btn-group.form-control').show();
            }
            $('#OtherProjectName').hide();
        }
        //$(this).closest("form").validate().element($('#ApartmentId'));
        //$(this).closest("form").validate().element($('#OtherProjectName'));
    });

    function updateOrtherName() {
        var slcApartmentId = $("#ApartmentId", $(this).closest('form'));
        var slcApartmentIds = $("#ApartmentIds", $(this).closest('form'));
        if ($('#ChkOtherProvinceName').is(':checked')) {
            $('#ProvinceId').val('').change().siblings('.combobox-container').hide();
            $('#OtherProvinceName').show();
        }
        if ($('#ChkOtherDistrictName').is(':checked')) {
            $('#DistrictId').siblings('.combobox-container').hide();
            $('#OtherDistrictName').show();
        }
        if ($('#ChkOtherWardName').is(':checked')) {
            $('#WardId').siblings('.combobox-container').hide();
            $('#OtherWardName').show();
        }
        if ($('#ChkOtherStreetName').is(':checked')) {
            $('#StreetId').siblings('.combobox-container').hide();
            $('#OtherStreetName').show();
        }
        if ($('#ChkOtherProjectName').is(':checked')) {
            $('#ApartmentId').siblings('.combobox-container').hide();
            $('#OtherProjectName').show();
            if (slcApartmentIds.length > 0) {
                slcApartmentIds.siblings('.btn-group.form-control').hide();
                slcApartmentIds.multiselect('rebuild');
            }
            slcApartmentId.siblings('.btn-group.form-control').hide();
            slcApartmentId.empty();
        }
    };

    updateOrtherName();

    // #endregion

    // AdsTypeCssClass change
    $('#AdsTypeCssClass').change(function () {
        if ($(this).val() == 'ad-leasing') $('#PaymentMethodId').val(57).change();
        else $('#PaymentMethodId').val(56).change();

        if ($('#TypeGroupCssClass').val() == "gp-house") {
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
    });
    // PropertyTypeGroup change
    $('#TypeGroupCssClass').change(function () {
        var typeGroupCssClass = $(this).val();
        if ($("#CurrentTypeGroupCssClass").length > 0) {
            loadTypeCssClass();
        } else {
            window.location = $('a.' + typeGroupCssClass).attr('href');
        }
    });
    // PropertyType change
    $('#TypeId').change(function () {
        var typeId = $(this).val();
        if (typeId == 231) {
            // 231 - 'tp-residential-land'
            // Đất thổ cư --> không có Thông tin xây dựng
            $('#tblConstruction').hide();
            $('#tblConstruction input').attr('disabled', 'disabled');
        }
        else {
            $('#tblConstruction').show();
            $('#tblConstruction input').removeAttr('disabled');
        }
        if (typeId > 0 && $("#CurrentTypeGroupCssClass").length > 0) {
            if ($("#TypeGroupCssClass").val() != $("#CurrentTypeGroupCssClass").val()) {
                $("[name='submit.SaveContinue']")[0].click();
            }
        }
        // Load TypeConstructions
        $('#Floors').change();
    }).change();

    // select Floors
    $('#FloorsCount').change(function () {
        if ($(this).val() == -1) {
            $('#Floors').closest('.control-group').show();
            $('#Floors').focus();
        } else {
            $('#Floors').val($(this).val()).closest('.control-group').hide();
        }
        $('#Floors').change();
    }).change();

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
                    $('.apartment_' + oldApartmentId).remove();
                    $('#oldAprartmentId').val(apartmentId);

                    $.each(response.apartmentList, function (i, item) {
                        if (item.Path != '') {
                            var checkedAvatar = '';
                            var fileName = item.Path.split('/');
                            if (item.IsAvatar) checkedAvatar = 'checked=\"checked\"';
                            $("ul.gallery").append(
                                '<li class="apartment_' + apartmentId + '"> \
                            <div class="image-wrapper"> \
                            <div class="image-container"> \
                                <a href="' + item.Path + '" target="_blank" rel="prettyPhoto[' + propertyId + ']"> \
                                    <img alt="' + fileName[fileName.length - 1] + '" src="' + item.Path + '" width="140" /> \
                                </a> \
                            </div> \
                            </div> \
                            <div class="image-control"> \
                                <label><input type="checkbox" name="chkPublished" checked="checked" id="chkPublished-' + item.Id + '" value="' + item.Id + '" >Published</label> \
                                <label><input type="radio" name="radIsAvatar" ' + checkedAvatar + ' id="radIsAvatar-' + item.Id + '" value="' + item.Id + '" >Avatar</label> \
                            </div> \
                        </li>'
                            );
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

                    if (apartmentId != oldApartmentId) {
                        $('#AddressNumber').val(response.apartmentAddressNumber).change();
                    }

                    $('#ApartmentFloors').val(response.apartmentFloors);
                    $('#ApartmentElevators').val(response.apartmentElevators);
                    $('#ApartmentBasements').val(response.apartmentBasements);
                    $('#Content').val(response.apartmentDescription);
                    if (typeof (tinyMCE) != "undefined" && apartmentId != oldApartmentId) {
                        tinyMCE.get('Content').setContent(response.apartmentDescription);
                    }
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
                    $('#ApartmentFloors, #ApartmentElevators, #ApartmentBasements').val('');
                    $('.apartment-adv-Childcare, .apartment-adv-Park, .apartment-adv-SwimmingPool, .apartment-adv-SuperMarket, .apartment-adv-SportCenter').prop('checked', false);
                    console.log(error);
                }
            });
        }
    }).change();

    // BĐS hiện trên trang chủ
    $('#Published').change(function () { if ($(this).is(':checked')) $('#AddAdsExpirationDate').val('Day90'); });

    function showPriceEstimatedByStaff() {
        if ($('#AdsGoodDeal').is(':checked')) {
            // Quảng cáo BĐS giá rẻ cho những BĐS chưa định giá được
            if ($('#FlagId').val() == 35 && $('#AdsTypeCssClass').val() == "ad-selling" && $('#TypeGroupCssClass').val() == 'gp-house') {
                $('#PriceEstimatedByStaff').closest('.control-group').show();
                // Giá định giá phải nhỏ hơn Giá rao
                if ($('#PriceEstimatedByStaff').val() == '' || $('#PriceEstimatedByStaff').val() < $('#PriceEstimatedByStaff').val()) {
                    $('#PriceEstimatedByStaff').focus();
                    $('html, body').animate({ scrollTop: $('#PriceEstimatedByStaff').closest('.content-item').offset().top }, 500);
                }
            } else {
                $('#PriceEstimatedByStaff').closest('.control-group').hide();
            }
        }
    }

    showPriceEstimatedByStaff();

    // BĐS giá rẻ
    $('#AdsGoodDeal').change(function () {
        if ($(this).is(':checked')) {
            $('#AddAdsGoodDealExpirationDate').val('Day90');
            showPriceEstimatedByStaff();
        } else {
            $('#PriceEstimatedByStaff').closest('.control-group').hide();
        }
    });

    // BĐS giao dịch gấp (VIP)
    $('#AdsVIP').change(function () {
        if ($(this).is(':checked')) {
            $('#AddAdsVIPExpirationDate').val('Day90'); /*ResetDateVip();*/
            $('#Published').attr('checked', true).change();
        }
    });

    // BĐS nổi bật
    $('#AdsHighlight').change(function () { if ($(this).is(':checked')) $('#AddAdsHighlightExpirationDate').val('Day90'); });

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
                for (var i = 0; i <= turns; i++) {
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
    $('#AlleyTurns').change(function () {
        if ($('#LocationCssClass').val() == "h-alley") {
            var turns = $(this).val();
            if (turns > 6) {
                turns = 6;
                $(this).val('6');
            }
            if (turns <= 0) {
                turns = 1;
                $(this).val('1');
            }
            $('[id^="divAlleyWidth"]').hide();
            for (var i = 0; i <= turns; i++) {
                $('[id^="divAlleyWidth' + i + '"]').show();
            }
        }
    }).change();

    // Price change
    $('#PriceProposed, #PaymentMethodId, #PaymentUnitId').change(function () { DocGiaRao(); });

    $('#PriceProposed').keyup(function () { DocGiaRao(); });

    DocGiaRao();

    // Estimate Property
    $('[name="submit.Estimate"]').click(function () { if (isNaN($('#PriceProposed').val()) || $('#PriceProposed').val() == '') $('#PriceProposed').val(-1); });

    //$('#PublishContact').change(function () { if ($(this).is(':checked')) $('#ContactPhoneToDisplay').attr('disabled', 'disabled'); else $('#ContactPhoneToDisplay').removeAttr('disabled', 'disabled'); }).change();

    // #endregion

    // #region AJAX Validate

    // Check exists property
    $("#WardId, #StreetId, #ApartmentId, #AddressNumber, #AddressCorner, #ApartmentNumber, #AdsTypeCssClass").change(function () {
        if (!$('#IsExternal').is(':checked')) {

            var id = parseInt($("#Id").html());
            if (isNaN(id)) id = 0;
            var provinceId = parseInt($("#ProvinceId").val());
            if (isNaN(provinceId)) provinceId = 0;
            var districtId = parseInt($("#DistrictId").val());
            if (isNaN(districtId)) districtId = 0;
            var wardId = parseInt($("#WardId").val());
            if (isNaN(wardId)) wardId = 0;
            var streetId = parseInt($("#StreetId").val());
            if (isNaN(streetId)) streetId = 0;
            var apartmentId = parseInt($("#ApartmentId").val());
            if (isNaN(apartmentId)) apartmentId = 0;
            var addressNumber = $("#AddressNumber").val();
            var addressCorner = $("#AddressCorner").is(':visible') ? $("#AddressCorner").val() : "";
            var apartmentNumber = $("#ApartmentNumber").val();
            var adsTypeCssClass = $("#AdsTypeCssClass").val();

            if (districtId > 0 &&
                (
                    (wardId > 0 && streetId > 0 && addressNumber != "" && addressNumber != null)
                        ||
                        (apartmentId > 0 && apartmentNumber != "" && apartmentNumber != null)
                )
            ) {
                $.ajax({
                    type: "post",
                    dataType: "",
                    url: "/RealEstate.Admin/Home/AjaxValidateAddress",
                    data: {
                        id: id,
                        provinceId: provinceId,
                        districtId: districtId,
                        wardId: wardId,
                        streetId: streetId,
                        apartmentId: apartmentId,
                        addressNumber: addressNumber,
                        addressCorner: addressCorner,
                        apartmentNumber: apartmentNumber,
                        adsTypeCssClass: adsTypeCssClass,
                        __RequestVerificationToken: antiForgeryToken
                    },
                    success: function (response) {
                        if (response.exists) {
                            var msg = '<div id="exists" class="message message-Warning">Bạn có muốn chỉnh sửa thông tin BĐS: <a href="' + response.link + '">' + response.address + '</a> ' + response.info + '</div>';
                            if ($('#messages').length == 0) $('#main').prepend('<div id="messages"><div class="zone zone-messages"></div></div>');
                            $('#messages .zone-messages').prepend(msg);

                            $('.primaryAction').attr('disabled', 'disabled').addClass('disabled');

                            switch (response.statusCssClass) {
                                case "st-sold":
                                case "st-onhold":
                                case "st-trash":
                                case "st-pending":
                                case "st-no-contact":
                                case "st-estimate":
                                    //alert("Địa chỉ: " + response.address + " đã có trong kho dữ liệu.\nBạn sẽ được tự động chuyển sang trang chỉnh sửa thông tin BĐS.");
                                    $('#overlay .overlay-content').html('<div class="message message-Warning" style="padding:10px;margin:0;"><h2>' + response.info + '</h2><h3>Click vào link để sửa thông tin:<br/> <a href="' + response.link + '">' + response.address + '</a></h3></div>');
                                    $('#overlay').show();
                                    //window.location = response.link;
                                    break;
                                default:
                                    alert("Địa chỉ: " + response.address + " đã có trong kho dữ liệu.");
                                    break;
                            }
                        } else {
                            $('#messages #exists').remove();
                            $('.primaryAction').removeAttr('disabled').removeClass('disabled');
                        }
                    },
                    error: function (request, status, error) {
                    }
                });
            }
        }
        return false;
    });

    // Check customer ContactPhone exists
    $("#ContactPhone.ajax-validate").change(function () {
        var id = parseInt($("#Id").html());
        if (isNaN(id)) id = 0;
        var contactPhone = $("#ContactPhone").val();
        if (contactPhone != "") {
            $.ajax({
                type: "post",
                dataType: "",
                url: "/RealEstate.Admin/Home/AjaxValidateContactPhone",
                data: {
                    id: id,
                    contactPhone: contactPhone,
                    __RequestVerificationToken: antiForgeryToken
                },
                success: function (response) {
                    if (response.exists) {
                        var msg = '<div id="exists" class="message message-Warning">Bạn có muốn chỉnh sửa thông tin KH: <a href="' + response.link + '">' + response.contactName + ' - ' + response.contactPhone + '</a></div>';
                        if ($('#messages').length == 0) $('#main').prepend('<div id="messages"><div class="zone zone-messages"></div></div>');
                        $('#messages .zone-messages').prepend(msg);

                        $('.primaryAction').attr('disabled', 'disabled').addClass('disabled');
                        alert("Khách hàng: " + response.contactName + ' - ' + response.contactPhone + " đã có trong kho dữ liệu.");
                    } else {
                        $('#messages #exists').remove();
                        $('.primaryAction').removeAttr('disabled').removeClass('disabled');
                    }
                },
                error: function (request, status, error) {
                }
            });
        }
        return false;
    });

    // Check exists street relation
    $("#DistrictId.ajax-validate-relation, #WardId.ajax-validate-relation, #StreetId.ajax-validate-relation").change(function () {
        var id = parseInt($("#Id").html());
        if (isNaN(id)) id = 0;
        var districtId = parseInt($("#DistrictId").val());
        var wardId = parseInt($("#WardId").val());
        var streetId = parseInt($("#StreetId").val());
        if (districtId > 0 && wardId > 0 && streetId > 0) {
            $.ajax({
                type: "post",
                dataType: "",
                url: "/RealEstate.Admin/Home/AjaxValidateStreetRelation",
                data: {
                    id: id,
                    districtId: districtId,
                    wardId: wardId,
                    streetId: streetId,
                    __RequestVerificationToken: antiForgeryToken
                },
                success: function (response) {
                    if (response.exists) {
                        var msg = '<div id="exists" class="message message-Warning">Bạn có muốn chỉnh sửa thông tin: <a href="' + response.link + '">' + response.name + '</a></div>';
                        if ($('#messages').length == 0) $('#main').prepend('<div id="messages"><div class="zone zone-messages"></div></div>');
                        $('#messages .zone-messages').prepend(msg);

                        $('.primaryAction').attr('disabled', 'disabled').addClass('disabled');
                        alert("Đường " + response.name + " đã có trong dữ liệu.");
                    } else {
                        $('#messages #exists').remove();
                        $('.primaryAction').removeAttr('disabled').removeClass('disabled');
                    }
                },
                error: function (request, status, error) {
                }
            });
        }
        return false;
    });

    // Check exists street relation
    $("#CoefficientAlleyId.ajax-validate-relation").change(function () {
        var coefficientAlleyId = parseInt($("#CoefficientAlleyId").val());
        if (coefficientAlleyId > 0) {
            $.ajax({
                type: "post",
                dataType: "",
                url: "/RealEstate.Admin/Home/AjaxGetCoefficientAlley",
                data: {
                    id: coefficientAlleyId,
                    __RequestVerificationToken: antiForgeryToken
                },
                success: function (response) {
                    if (response.success) {
                        $('#CoefficientAlley1Max').val(response.H1max);
                        $('#CoefficientAlley1Min').val(response.H1min);
                        $('#CoefficientAlleyEqual').val(response.Hequal);
                        $('#CoefficientAlleyMax').val(response.Hmax);
                        $('#CoefficientAlleyMin').val(response.Hmin);
                    } else {
                        $('#CoefficientAlley1Max').val('');
                        $('#CoefficientAlley1Min').val('');
                        $('#CoefficientAlleyEqual').val('');
                        $('#CoefficientAlleyMax').val('');
                        $('#CoefficientAlleyMin').val('');
                    }
                },
                error: function (request, status, error) {
                }
            });
        }
        return false;
    });

    // #endregion

    // #region FORM ACTION

    // Change AdsType
    $('#tabsAdsType a').click(function () {
        $(this).parent().addClass('active').siblings().removeClass('active');
        $('#AdsTypeId').val($(this).attr('ads'));
        $('#TypeGroupId').val($(this).attr('typegroup'));
        if ($(this).is(':contains("thuê")'))
            $('#PaymentMethodId option:contains("Triệu"):first').attr('selected', 'selected');
        else
            $('#PaymentMethodId option:contains("Tỷ")').attr('selected', 'selected');

        // submit form
        var $form = $(this).closest('form');
        $form.find("input[type=text]").val("");
        $form.find('#ProvinceId').change();
        $form.find('.multiselect').multiselect('rebuild');
        $form.submit();
        return false;
    });

    // Fixed pager link
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
        for (var k = 0; k < arrToRemove.length; k++) {
            removeByValue(arrParams, arrToRemove[k]);
        }
        arrParams = arrParams.concat(arrToAdd);
        $(this).attr('href', strDomain + arrParams.join('&'));
    });

    // auto submit form when select change
    $("#publishActions, .publishActions").change(function () {
        var action = $(this).val();
        switch (action) {
            case 'Export':
                if ($(':checkbox[name^=Properties]:checked').length > 0) {
                    $('button[name="submit.BulkEdit"]').click();
                } else {
                    //$(this).closest("form").submit();
                    alert('Vui lòng chọn các BĐS cần đăng tin.');
                    $(this).val('');
                    return false;
                }
                break;
            case 'UpdateNegotiateStatus':
                if (confirm("Chuyển các BĐS [Đang thương lượng] quá thời gian 7 ngày thành [Đang rao bán] ?")) {
                    $('#ReturnUrl').val(location);
                    $('button[name="submit.BulkEdit"]').click();
                }
                break;
            case 'UpdateMetaDescriptionKeywords':
                if (confirm("Cập nhật lại toàn bộ meta Decription Keywords BĐS?")) {
                    $('#ReturnUrl').val(location);
                    $('button[name="submit.BulkEdit"]').click();
                }
                break;
                /*case 'UpdatePlacesAround':
                        if (confirm("Cập nhật lại toàn bộ Địa điểm xung quanh BĐS?")) {
                        $('#ReturnUrl').val(location);
                        $('button[name="submit.BulkEdit"]').click();
                        }
                        break;*/
            default:
                if ($(':checkbox[name$=".IsChecked"]:checked').length > 0) {
                    $('#ReturnUrl').val(location);
                    //$('button[name="submit.BulkEdit"]').click();
                } else {
                    alert('Vui lòng chọn dữ liệu cần cập nhật.');
                    $(this).val('');
                }
                break;
        }
        return true;
    });

    $("form[method=get]").submit(function () {

        $('input[type=text], select, input[type=hidden]', this).each(function () { this.disabled = ($(this).val() == 'false' || $(this).val() == ''); });

        $('input[type=checkbox]', this).each(function () { this.disabled = !($(this).is(':checked')); });


        if ($('#PaymentMethodId').val() == 56) $('#PaymentMethodId').attr('disabled', 'disabled');
        if ($('#PaymentUnitId').val() == 89) $('#PaymentUnitId').attr('disabled', 'disabled');

        if ($('#Order').val() == 'LastUpdatedDate') $('#Order').attr('disabled', 'disabled');

        $('button[name="submit.Filter"]').attr('disabled', 'disabled');
        $('input[name^=autocomplete]').attr('disabled', 'disabled');
        $('input[name="__RequestVerificationToken"]').attr('disabled', 'disabled');

    });

    $('.form-serialize').each(function () { $(this).data('serialize', $(this).serialize()); });

    $('#YoutubeUpload').on('change', function () {
        if ($(this).length > 0) {
            // Form has changed!!!
            $('#IsChanged').attr("checked", "checked");
        }
    });

    $("form").submit(function () {
        // Disable empty value
        if (isNaN($('#Latitude').val())) $('#Latitude').val(0);
        if (isNaN($('#Longitude').val())) $('#Longitude').val(0);

        if ($(this).hasClass("form-serialize")) {
            if ($(this).serialize() != $(this).data('serialize')) {
                // Form has changed!!!
                $('#IsChanged').attr("checked", "checked");
            }
        }

        $('input[type="submit"], .button').prop('disabled', true).click(function () { return false; });

    });

    // #endregion

    $('#AddAdsVIPExpirationDate').change(function () {
        var value = $(this).val();
        var days;
        switch (value) {
            case 'Day10':
                days = 10;
                break;
            case 'Day20':
                days = 20;
                break;
            case 'Day30':
                days = 30;
                break;
            case 'Day60':
                days = 60;
                break;
            case 'Day90':
                days = 90;
                break;
            default:
                days = 0;
                break;
        }
    });
});

$(function () {


    // #region INIT Dialog
    if ($.fn.dialog) {
        $('#dialog:ui-dialog').dialog('destroy');

        // init dialog
        $('div[id^=dialog]').removeClass('ui-dialog').dialog({ autoOpen: false, closeOnEscape: true, resizable: false, modal: true, width: 'auto' });

        // dialog-trigger
        $(document).on('click', 'a.dialog-trigger', function () {
            $($(this).attr('href')).dialog('open');
            return false;
        });
        $(document).on('click', 'button.dialog-trigger', function () {
            $($(this).val()).dialog('open');
            return false;
        });
    }
    // #endregion

    // Load calendar
    if ($.fn.datepicker) {
        $('.date-box').datepicker({
            //showOn: "both",
            //buttonImageOnly: true,
            //buttonText: '',
            //buttonImage: "images/icons/calendar_ico.gif",
            dateFormat: "dd/mm/yy",
            changeMonth: true,
            changeYear: true,
            numberOfMonths: 1,
            onSelect: function (selectedDate) {
                var elId = $(this).attr('id');
                if (elId.indexOf('From') >= 0)
                    $('#' + $(this).attr('id').replace('From', 'To')).datepicker('option', 'minDate', selectedDate);
                else if (elId.indexOf('To') >= 0)
                    $('#' + $(this).attr('id').replace('To', 'From')).datepicker("option", "maxDate", selectedDate);
            }
        });
    }

    // Load float table header
    if ($.fn.floatHeader) {
        $('.float-header').floatHeader({ fadeIn: 250, fadeOut: 250 });
    }

    // Check IsCopyable
    $('tr[data-id]').each(function () {
        /*
        $.ajax({
        type: "post",
        dataType: "",
        url: "/RealEstate.Admin/Home/AjaxIsCopyable",
        data: {
        id: $(this).attr('data-id'),
        __RequestVerificationToken: antiForgeryToken
        },
        success: function (response) {
        if (response.isCopyable == true) {
        $('tr[data-id=' + response.id + ']').addClass('bg-resolved');
        }
        },
        error: function (request, status, error) {
        }
        });
        */
    });

    // Check BĐS Chờ duyệt
    if ($('.section-b-s-ch-duy-t').length > 0) {
        $('.section-b-s-ch-duy-t').hide();
        window.addEventListener("DOMContentLoaded", checkPendingProperties, false);
    }

    // Check Khách hàng Chờ duyệt
    if ($('.section-k-h-ch-duy-t').length > 0) {
        $('.section-k-h-ch-duy-t').hide();
        window.addEventListener("DOMContentLoaded", checkPendingCustomers, false);
    }

    // Customer  PropertyTypeGroup Change
    $('#PropertyTypeGroupId').change(function () {
        var typeGroupId = $(this).val();
        if (typeGroupId == 52) // if is Apartment
        {
            $('.apartment-name, .apartment-floor').show();
            $('.not-apartment-floor input').attr('disabled', 'disabled');
            $('.apartment-floor input').removeAttr('disabled');

            $('.not-apartment').hide();
            $("#PaymentMethodId option[value=57]").prop("selected", true); //Triệu VNĐ
        } else {
            $('.not-apartment').show();
            $('.apartment-name, .apartment-floor').hide();

            $('.not-apartment-floor input').removeAttr('disabled');
            $('.apartment-floor input').attr('disabled', 'disabled');

            $("#PaymentMethodId option[value=56]").prop("selected", true); //Tỷ VNĐ
        }
    }).change();
    // Customer  AdsType Change
    $('#AdsTypeId').change(function () {
        var adsTypeId = $(this).val();
        if (adsTypeId == 98466) // if is ad-renting
        {
            $("#PaymentMethodId option[value=57]").prop("selected", true); //Triệu VNĐ
        } else if (adsTypeId == 98465) { // if is ad-buying
            $("#PaymentMethodId option[value=56]").prop("selected", true); //Tỷ VNĐ
        }
    }).change();

    //Delete Video
    $('.delYoutubeVideo').click(function () {
        if (confirm('Bạn có chắc muốn xóa ?')) {
            var that = $(this);
            $.ajax({
                type: "post",
                dataType: "",
                url: "/RealEstate.Admin/Home/AjaxDelYoutube",
                data: {
                    propertyId: $(this).data('id'),
                    __RequestVerificationToken: antiForgeryToken
                },
                success: function (response) {
                    if (response.status) {
                        $(that).closest('.content-item').remove();
                        $('#YoutubeId').val('');
                    }
                },
                error: function (request, status, error) {
                }
            });
        }
    })
});

// URL Params
function getURLParameter(name) { return decodeURI((RegExp(name + '=' + '(.+?)(&|$)').exec(location.search) || [, null])[1]); }

function removeByValue(arr, val) {
    for (var i = 0; i < arr.length; i++) {
        if (arr[i] == val) {
            arr.splice(i, 1);
            break;
        }
    }
}

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

// Checking Pending Items

function checkPendingProperties() {

    var countPendingInDetails = $('.local-section-b-s-ch-duy-t.selected').length > 0;
    //var countAdsRequestInDetails = $('.local-section-b-s-ng-v-i-p.selected').length > 0;

    var source = new EventSource('/RealEstate.Admin/Home/AjaxCountPendingProperties/?countPendingInDetails=' + countPendingInDetails);

    source.onmessage = function (msg) {

        var response = JSON.parse(msg.data);

        if ($('.navicon-b-s-n-i-b').length > 0) $('a.navicon-b-s-n-i-b').html('BĐS nội bộ \
                            <a href="/Admin?ProvinceId=0&StatusId=29" title="BĐS Chờ duyệt">(' + response.countPendingProperties + ') \
                            <a href="/Admin?ProvinceId=0&StatusId=0&AdsRequest=True" title="BĐS đăng VIP">(' + response.countAdsRequestProperties + ') \
                            </a>');

        // BĐS Chờ duyệt
        if ($('.local-section-b-s-ch-duy-t').length > 0) {
            $('.local-section-b-s-ch-duy-t a').text('BĐS Chờ duyệt (' + response.countPendingProperties + ')');
            if ($('.local-section-b-s-ch-duy-t').hasClass('selected')) {

                if ($('.ad-selling-gp-house').length > 0) $('.ad-selling-gp-house .count').text('(' + response.countPendingSellingHouse + ')');
                if ($('.ad-selling-gp-apartment').length > 0) $('.ad-selling-gp-apartment .count').text('(' + response.countPendingSellingApartment + ')');
                if ($('.ad-selling-gp-land').length > 0) $('.ad-selling-gp-land .count').text('(' + response.countPendingSellingLand + ')');

                if ($('.ad-leasing-gp-house').length > 0) $('.ad-leasing-gp-house .count').text('(' + response.countPendingLeasingHouse + ')');
                if ($('.ad-leasing-gp-apartment').length > 0) $('.ad-leasing-gp-apartment .count').text('(' + response.countPendingLeasingApartment + ')');
                if ($('.ad-leasing-gp-land').length > 0) $('.ad-leasing-gp-land .count').text('(' + response.countPendingLeasingLand + ')');
            }
        }

        // BĐS Quảng cáo
        //if ($('.local-section-b-s-ng-v-i-p').length > 0) {
        //    $('.local-section-b-s-ng-v-i-p a').text('BĐS đăng VIP (' + response.countAdsRequestProperties + ')');
        //    if ($('.local-section-b-s-ng-v-i-p').hasClass('selected')) {

        //        if ($('.ad-selling-gp-house').length > 0) $('.ad-selling-gp-house .count').text('(' + response.countAdsRequestSellingHouse + ')');
        //        if ($('.ad-selling-gp-apartment').length > 0) $('.ad-selling-gp-apartment .count').text('(' + response.countAdsRequestSellingApartment + ')');
        //        if ($('.ad-selling-gp-land').length > 0) $('.ad-selling-gp-land .count').text('(' + response.countAdsRequestSellingLand + ')');

        //        if ($('.ad-leasing-gp-house').length > 0) $('.ad-leasing-gp-house .count').text('(' + response.countAdsRequestLeasingHouse + ')');
        //        if ($('.ad-leasing-gp-apartment').length > 0) $('.ad-leasing-gp-apartment .count').text('(' + response.countAdsRequestLeasingApartment + ')');
        //        if ($('.ad-leasing-gp-land').length > 0) $('.ad-leasing-gp-land .count').text('(' + response.countAdsRequestLeasingLand + ')');
        //    }
        //}

    }
}

function checkPendingCustomers() {

    var countPendingInDetails = $('.local-section-k-h-ch-duy-t.selected').length > 0;

    var source = new EventSource('/RealEstate.Admin/Home/AjaxCountPendingCustomers/?countPendingInDetails=' + countPendingInDetails);

    source.onmessage = function (msg) {

        var response = JSON.parse(msg.data);

        if ($('.navicon-kh-ch-h-ng').length > 0) $('a.navicon-kh-ch-h-ng').html('Khách hàng <a href="/Admin/RealEstate/Customer?StatusId=132823" title="KH Chờ duyệt">(' + response.countPendingCustomers + ')</a>');

        // KH Chờ duyệt
        if ($('.local-section-k-h-ch-duy-t').length > 0) {
            $('.local-section-k-h-ch-duy-t a').text('KH Chờ duyệt (' + response.countPendingCustomers + ')');
            if ($('.local-section-k-h-ch-duy-t').hasClass('selected')) {

                if ($('.ad-buying-gp-house').length > 0) $('.ad-buying-gp-house .count').text('(' + response.countPendingBuyingHouse + ')');
                if ($('.ad-buying-gp-apartment').length > 0) $('.ad-buying-gp-apartment .count').text('(' + response.countPendingBuyingApartment + ')');
                if ($('.ad-buying-gp-land').length > 0) $('.ad-buying-gp-land .count').text('(' + response.countPendingBuyingLand + ')');

                if ($('.ad-renting-gp-house').length > 0) $('.ad-renting-gp-house .count').text('(' + response.countPendingRentingHouse + ')');
                if ($('.ad-renting-gp-apartment').length > 0) $('.ad-renting-gp-apartment .count').text('(' + response.countPendingRentingApartment + ')');
                if ($('.ad-renting-gp-land').length > 0) $('.ad-renting-gp-land .count').text('(' + response.countPendingRentingLand + ')');

                if ($('.ad-exchange-gp-house').length > 0) $('.ad-exchange-gp-house .count').text('(' + response.countPendingExchangeHouse + ')');
                if ($('.ad-exchange-gp-apartment').length > 0) $('.ad-exchange-gp-apartment .count').text('(' + response.countPendingExchangeApartment + ')');
                if ($('.ad-exchange-gp-land').length > 0) $('.ad-exchange-gp-land .count').text('(' + response.countPendingExchangeLand + ')');
            }
        }

    }
}

/*
* Convert money from numbers to words in VND
*
*/

// #region Convert money from numbers to words in VND

function DocGiaRao() {
    var number = parseFloat($('#PriceProposed').val());
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
        var numberStr = number.toString();
        number = numberStr.substring(0, numberStr.indexOf('.'));

        var value = DocTienBangChu(number) + " " + unitText + " / " + $('#PaymentUnitId option:selected').text();

        $('#PriceInWords').html(value);
    } else {
        $('#PriceInWords').html('');
    }
}

var ChuSo = new Array(" không ", " một ", " hai ", " ba ", " bốn ", " năm ", " sáu ", " bảy ", " tám ", " chín ");
var Tien = new Array("", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ");

//1. Hàm đọc số có ba chữ số;
function DocSo3ChuSo(baso) {
    var tram = parseInt(baso / 100);
    var chuc = parseInt((baso % 100) / 10);
    var donvi = baso % 10;
    var ketQua = "";
    if (tram == 0 && chuc == 0 && donvi == 0) return "";
    if (tram != 0) {
        ketQua += ChuSo[tram] + " trăm ";
        if ((chuc == 0) && (donvi != 0)) ketQua += " linh ";
    }
    if ((chuc != 0) && (chuc != 1)) {
        ketQua += ChuSo[chuc] + " mươi";
        if ((chuc == 0) && (donvi != 0)) ketQua = ketQua + " linh ";
    }
    if (chuc == 1) ketQua += " mười ";
    switch (donvi) {
        case 1:
            if ((chuc != 0) && (chuc != 1)) {
                ketQua += " mốt ";
            } else {
                ketQua += ChuSo[donvi];
            }
            break;
        case 5:
            if (chuc == 0) {
                ketQua += ChuSo[donvi];
            } else {
                ketQua += " lăm ";
            }
            break;
        default:
            if (donvi != 0) {
                ketQua += ChuSo[donvi];
            }
            break;
    }
    return ketQua;
}

//2. Hàm đọc số thành chữ (Sử dụng hàm đọc số có ba chữ số)

function DocTienBangChu(soTien) {
    var lan = 0;
    var i = 0;
    var so = 0;
    var ketQua = "";
    var tmp = "";
    var viTri = new Array();
    if (soTien < 0) return "Số tiền âm !";
    if (soTien == 0) return "Không đồng !";
    if (soTien > 0) {
        so = soTien;
    } else {
        so = -soTien;
    }
    if (soTien > 8999999999999999) {
        return "Số quá lớn!";
    }
    viTri[5] = Math.floor(so / 1000000000000000);
    if (isNaN(viTri[5])) viTri[5] = "0";
    so = so - parseFloat(viTri[5].toString()) * 1000000000000000;
    viTri[4] = Math.floor(so / 1000000000000);
    if (isNaN(viTri[4])) viTri[4] = "0";
    so = so - parseFloat(viTri[4].toString()) * 1000000000000;
    viTri[3] = Math.floor(so / 1000000000);
    if (isNaN(viTri[3])) viTri[3] = "0";
    so = so - parseFloat(viTri[3].toString()) * 1000000000;
    viTri[2] = parseInt(so / 1000000);
    if (isNaN(viTri[2])) viTri[2] = "0";
    viTri[1] = parseInt((so % 1000000) / 1000);
    if (isNaN(viTri[1])) viTri[1] = "0";
    viTri[0] = parseInt(so % 1000);
    if (isNaN(viTri[0])) viTri[0] = "0";
    if (viTri[5] > 0) {
        lan = 5;
    } else if (viTri[4] > 0) {
        lan = 4;
    } else if (viTri[3] > 0) {
        lan = 3;
    } else if (viTri[2] > 0) {
        lan = 2;
    } else if (viTri[1] > 0) {
        lan = 1;
    } else {
        lan = 0;
    };
    for (i = lan; i >= 0; i--) {
        tmp = DocSo3ChuSo(viTri[i]);
        ketQua += tmp;
        if (viTri[i] > 0) ketQua += Tien[i];
        if ((i > 0) && (tmp.length > 0)) ketQua += ','; /*&& (!string.IsNullOrEmpty(tmp))*/
    };
    if (ketQua.substring(ketQua.length - 1) == ',') {
        ketQua = ketQua.substring(0, ketQua.length - 1);
    }
    ketQua = ketQua.substring(1, 2).toUpperCase() + ketQua.substring(2);

    // Đọc phần thập phân
    //strPhanThapPhan = SoTien.toString();
    //floatingPointIndex = strPhanThapPhan.indexOf('.');
    //var PhanThapPhan = "";
    //if (floatingPointIndex > -1) PhanThapPhan = "phẩy " + DocTienBangChu(strPhanThapPhan.substring(floatingPointIndex + 1));

    return ketQua; //.substring(0, 1);//.toUpperCase();// + KetQua.substring(1);
}

// #endregion

$('#FloorFrom, #FloorTo').change(function () {
    var floorFrom = Number($('#FloorFrom').val());
    var floorTo = Number($('#FloorTo').val());

    if (floorFrom > 0 && floorTo > 0 && floorTo - floorFrom >= 1) {
        var n = Number(floorTo - floorFrom)
        $('#price-difference').show();
        var htmlPrice = '';
        for (var i = floorFrom ; i <= floorTo; i++) {
            htmlPrice += '<tr>\
                                <td><input type="text" id="ToPosition' + i + '" name="ToPosition' + i + '" placeholder="Tầng" class="text text-box" value="' + i + '" /></td>\
                                <td><input type="text" id="Coefficient' + i + '" name="Coefficient' + i + '" placeholder="hệ số" class="text text-box" /></td>\
                            </tr>';
        }
        $('#tbl-pricedifference tbody').html(htmlPrice);
    }
    else {
        $('#tbl-pricedifference tbody').html('');
    }
    if (floorFrom > 0 && floorTo > 0) {
        if (floorTo - floorFrom < 0)
            alert('Giá trị nhập vào không đúng!');
    }
    else if (floorFrom == 0 && floorTo == 0) alert('Giá trị nhập vào không đúng!.');
});

$('#RoomInFloor').change(function () {
    var n = $(this).val();
    var blockId = $("#ApartmentBlockId", $(this).closest('form')).val();
    if (blockId == null || blockId == 0) {
        alert('Vui lòng chọn Block dự án');
        return false;
    }

    if (n > 0) {
        var options = '';
        $.ajax({
            type: "get",
            dataType: "",
            url: "/RealEstate.Admin/Home/AjaxGetListApartmentBlockInfo",
            data: {
                blockId: blockId
            },
            success: function (response) {
                $.each(response.list, function (i, item) {
                    options += '<option value="' + item.Value + '">' + item.Text + '</option>';
                });

                $('#apartment-info').show();
                var htmlApartmentInfo = '';
                for (var i = 1 ; i <= n; i++) {
                    var select = '<select name="ApartmentName' + i + '" style="width: 100px; margin-right: 10px;">' + options + '</select>';

                    htmlApartmentInfo += '<tr>\
                                    <td>Căn '+ i + '</td>\
                                    <td>' + select + '</td>\
                                    <td><input type="text" id="ApartmentCoefficient' + i + '" name="ApartmentCoefficient' + i + '" placeholder="Tổng hệ số" class="text text-box" /></td>\
                                </tr>';
                }
                $('#tbl-apartment tbody').html(htmlApartmentInfo);
            },
            error: function (request, status, error) {
            }
        });
    } else {
        alert('Giá trị nhập vào không đúng');
    }
});

$('.apartment-cart-form').submit(function () {
    var areaFlag = false;

    $("#tbl-apartment input[type='text']").each(function () {
        if ($(this).val() == '') areaFlag = true;
    });

    if (areaFlag) alert('Vui lòng nhập đầy đủ thông tin');

    if (areaFlag) return false;
});

$(".saveuserproperty").on("click", function () {
    var that = $(this);
    $.ajax({
        type: "post",
        dataType: "JSON",
        traditional: true,
        url: "/ajax/RealEstate.FrontEnd/PropertySearch/SaveProperty",
        data: {
            propertyid: $(that).data('id'),
            __RequestVerificationToken: antiForgeryToken
        },
        success: function (response) {
            if (response.success == true) {
                $(that).remove();
                alert("Lưu tin thành công");
            }
            else {
                if (response.flaguser == true) {
                    alert("Tin này đã được lưu.");
                    $(that).remove();
                }
                else {
                    //window.location.href = '@Url.Action("AccessDenied", "Account", new { area = "Orchard.Users", ReturnUrl = HttpContext.Current.Request.Url })';
                }
            }
        },
        error: function (request, status, error) {
        }
    });
});

$(".deleteuserproperty").on("click", function () {
    var that = $(this);
    $.ajax({
        type: "post",
        dataType: "JSON",
        traditional: true,
        url: "/ajax/RealEstate.FrontEnd/PropertySearch/DeleteUserProperty",
        data: {
            propertyid: $(that).data('id'),
            __RequestVerificationToken: antiForgeryToken
        },
        success: function (response) {
            if (response.success == true) {
                $(that).remove();
                alert("Đã xóa thành công");
            }
        },
        error: function (request, status, error) {
        }
    });
});

/** ExchangeTypeCssClass **/
$('#ExchangeTypeClass').change(function () {
    var that = $(this);
    $('#ExchangeValue').closest('.control-group').toggle(!($(that).val() == '' || $(that).val() == 'exchange-parity'));
    if ($(that).val() != null && $(that).val() == 'exchange-more') {
        $('.label-exchange').text('Số tiền bù thêm:');
    }
    if ($(that).val() != null && $(that).val() == 'exchange-lower') {
        $('.label-exchange').text('Số tiền muốn nhận thêm:');
    }
}).change();

$(function () {

    if ($.fn.select2) {

        var formatSelection = function (item) {
            return item.name
        }

        var formatResult = function (item) {
            return '<div class="select2-result">' + item.name + '</div>'
        }

        var initSelection = function (element, callback) {
            var id = $(element).val();
            if (id !== "") {
                // User
                if (element.is('.select2-user'))
                    $.ajax("/RealEstate.Admin/Home/GetUserForJson/" + id, { dataType: "json" }).done(function (data) { callback(data); });

                // Group
                if (element.is('.select2-group'))
                    $.ajax("/RealEstate.Admin/Home/GetGroupForJson/" + id, { dataType: "json" }).done(function (data) { callback(data); });
            }
        }

        $('.select2-user,.select2-group').select2({
            language: "vi",
            minimumInputLength: 2,
            multiple: false,
            quietMillis: 100,
            id: function (item) { return item.id; },
            ajax: {
                url: function () {

                    // User
                    if ($(this).is('.select2-user-available-group-member'))
                        return "/RealEstate.Admin/Home/SearchAvailableGroupMemberUsersForJson"
                    if ($(this).is('.select2-user-available-group-admin'))
                        return "/RealEstate.Admin/Home/SearchAvailableGroupAdminUsersForJson"
                    if ($(this).is('.select2-group-user'))
                        return "/RealEstate.Admin/Home/SearchGroupUsersForJson"
                    if ($(this).is('.select2-user'))
                        return "/RealEstate.Admin/Home/SearchUsersForJson";

                    // Group
                    if ($(this).is('.select2-group-seeder'))
                        return "/RealEstate.Admin/Home/SearchSeederGroupsForJson";
                    if ($(this).is('.select2-group'))
                        return "/RealEstate.Admin/Home/SearchGroupsForJson";
                    
                },
                dataType: 'json',
                type: 'GET',
                data: function (term, page) {
                    return {
                        q: term,
                        page: page || 1,
                        g: $(this).attr('group')
                    }
                },
                results: function (data, page) {
                    return { results: data.items, more: (data.items && data.items.length == 10 ? true : false) }
                }
            },
            formatResult: formatResult,
            formatSelection: formatSelection,
            initSelection: initSelection,
            allowClear: true
        })

    }
});
