$(function () {
    $("#DirectionIds").dropdownchecklist({ icon: {}, width: _defaultWidth, maxDropHeight: _maxDropHeight, emptyText: "Chọn tất cả Hướng BĐS " });
    $("#AnyType").dropdownchecklist({ icon: {}, width: _defaultWidth, maxDropHeight: _maxDropHeight, emptyText: "Tìm tất cả các trạng thái " });

    //Disable

    //refresh control
    $("#btnrefresh").click(function () {
        disableDropdownChecklist($("#DistrictIds"));
        disableDropdownChecklist($("#WardIds"));
        disableDropdownChecklist($("#StreetIds"));

        $("#AdsTypeCssClass").val('').change();
        $("#TypeGroupCssClass").val('').change();
        $("#ProvinceId").val(0).combobox("refresh").change();
        $("#DirectionIds").val("");
        //AnyType
        $("#AnyType").val("");
        $("#ApartmentFloorTh").val("").change();
        //AlleyTurnsRange
        $("#AlleyTurnsRange").val("").change();

        $("#OtherProjectName").val("");
        $("#MinAreaTotal").val("");
        $("#MinAreaUsable").val("");
        $("#MinAreaTotalWidth").val("");
        $("#MinAreaTotalLength").val("");
        $("#MinFloors").val("");
        $("#MinBedrooms").val("");
        $("#MinPriceProposed").val("");
        $("#MaxPriceProposed").val("");

        return false;
    });

    initFormFilter();
    $('#formFilter input, select').change(function () { initFormFilter(); });
});
function initFormFilter() {
    $('#DistrictIds').closest('.control-group').toggle($('#ProvinceId').val() > 0);
    $('#WardIds').closest('.control-group').toggle($('#DistrictIds').val() > 0);
    $('#StreetIds').closest('.control-group').toggle($('#DistrictIds').val() > 0);

    // show on gp-apartment
    $('#OtherProjectName, #ApartmentFloorThRange, #MinBedrooms').closest('.control-group').toggle($('#TypeGroupCssClass').val() == "gp-apartment");

    // not show on gp-apartment
    $('#MinAreaTotal').closest('.control-group').toggle($('#TypeGroupCssClass').val() != "gp-apartment");

    // show on gp-house, gp-land
    $('#AlleyTurnsRange, #MinAreaTotalWidth, #MinAreaTotalLength').closest('.control-group').toggle($('#TypeGroupCssClass').val() == "gp-house" || $('#TypeGroupCssClass').val() == "gp-land");

    // show on gp-house
    $('#MinFloors').closest('.control-group').toggle($('#TypeGroupCssClass').val() == "gp-house");
}
 _defaultWidth = 208;
