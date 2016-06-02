/* =============================================================
 * bootstrap-combobox.js v1.1.5
 * =============================================================
 * Copyright 2012 Daniel Farrell
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * ============================================================ */

!function( $ ) {

 "use strict";

 /* COMBOBOX PUBLIC CLASS DEFINITION
  * ================================ */

  var Combobox = function ( element, options ) {
    this.options = $.extend({}, $.fn.combobox.defaults, options);
    this.$source = $(element);
    this.$container = this.setup();
    this.$element = this.$container.find('input[type=text]');
    this.$target = this.$container.find('input[type=hidden]');
    this.$button = this.$container.find('.dropdown-toggle');
    this.$menu = $(this.options.menu).appendTo('body');
    this.template = this.options.template || this.template
    this.matcher = this.options.matcher || this.matcher;
    this.sorter = this.options.sorter || this.sorter;
    this.highlighter = this.options.highlighter || this.highlighter;
    this.shown = false;
    this.selected = false;
    this.refresh();
    this.transferAttributes();
    this.listen();
  };

  Combobox.prototype = {

    constructor: Combobox

  , setup: function () {
      var combobox = $(this.template());
      this.$source.before(combobox);
      this.$source.hide();
      return combobox;
    }

  , disable: function() {
      this.$element.prop('disabled', true);
      this.$button.attr('disabled', true);
      this.disabled = true;
      this.$container.addClass('combobox-disabled');
    }

  , enable: function() {
      this.$element.prop('disabled', false);
      this.$button.attr('disabled', false);
      this.disabled = false;
      this.$container.removeClass('combobox-disabled');
    }
  , parse: function () {
      var that = this
        , map = {}
        , source = []
        , selected = false
        , selectedValue = '';
      this.$source.find('option').each(function() {
        var option = $(this);
        if (option.val() === '') {
          that.options.placeholder = option.text();
          return;
        }
        map[option.text()] = option.val();
        source.push(option.text());
        if (option.prop('selected')) {
          selected = option.text();
          selectedValue = option.val();
        }
      })
      this.map = map;
      if (selected) {
        this.$element.val(selected);
        this.$target.val(selectedValue);
        this.$container.addClass('combobox-selected');
        this.selected = true;
      }
      return source;
    }

  , transferAttributes: function() {
    this.options.placeholder = this.$source.attr('data-placeholder') || this.options.placeholder
    this.$element.attr('placeholder', this.options.placeholder)
    this.$target.prop('name', this.$source.prop('name'))
    this.$target.val(this.$source.val())
    this.$element.prop('name', 'autocomplete' + this.$source.prop('name'))
    this.$element.attr('required', this.$source.attr('required'))
    this.$element.attr('data-msg-required', this.$source.attr('data-msg-required'));
    this.$element.attr('rel', this.$source.attr('rel'))
    this.$element.attr('title', this.$source.attr('title'))
    this.$element.attr('class', this.$source.attr('class'))
    this.$element.attr('tabindex', this.$source.attr('tabindex'))
    this.$source.removeAttr('tabindex')
    this.$source.removeAttr('name')  // Remove from source otherwise form will pass parameter twice.
    if (this.$source.attr('disabled')!==undefined)
      this.disable();
  }

  , select: function () {
      var val = this.$menu.find('.active').attr('data-value');
      this.$element.val(this.updater(val)).trigger('change');
      this.$target.val(this.map[val]).trigger('change');
      this.$source.val(this.map[val]).trigger('change');
      this.$container.addClass('combobox-selected');
      this.selected = true;
      if ($.fn.valid) this.$element.valid() // validate 
      return this.hide();
    }

  , updater: function (item) {
      return item;
    }

  , show: function () {
      var pos = $.extend({}, this.$element.position(), {
        height: this.$element[0].offsetHeight
      });

      this.$menu
        .insertAfter(this.$element)
        .css({
          top: pos.top + pos.height
        , left: pos.left
        })
        .show();

      $('.dropdown-menu').on('mousedown', $.proxy(this.scrollSafety, this));

      this.shown = true;
      return this;
    }

  , hide: function () {
      this.$menu.hide();
      $('.dropdown-menu').off('mousedown', $.proxy(this.scrollSafety, this));
      this.$element.on('blur', $.proxy(this.blur, this));
      this.shown = false;
      return this;
    }

  , lookup: function (event) {
      this.query = this.$element.val();
      return this.process(this.source);
    }

  , lookupAll: function (event) {
      this.query = '';
      return this.process(this.source);
  }

  , process: function (items) {
      var that = this;

      items = $.grep(items, function (item) {
        return that.matcher(item);
      })

      items = this.sorter(items);

      if (!items.length) {
        return this.shown ? this.hide() : this;
      }

      return this.render(items.slice(0, this.options.items)).show();
    }

  , template: function() {
      if (this.options.bsVersion == '2') {
        return '<div class="combobox-container"><input type="hidden" /> <div class="input-append"> <input type="text" autocomplete="off" /> <span class="add-on dropdown-toggle" data-dropdown="dropdown"> <span class="caret"/> <i class="icon-remove"/> </span> </div> </div>'
      } else {
        return '<div class="combobox-container"> <input type="hidden" /> <div class="input-group"> <input type="text" autocomplete="off" /> <span class="btn input-group-addon dropdown-toggle" data-dropdown="dropdown"> <span class="caret" /> <span class="glyphicon glyphicon-remove" /> </span> </div> </div>'
      }
    }

  , matcher: function (item) {
      return ~item.toLowerCase().indexOf(this.query.toLowerCase());
    }

  , sorter: function (items) {
      var beginswith = []
        , caseSensitive = []
        , caseInsensitive = []
        , item;

      while (item = items.shift()) {
        if (!item.toLowerCase().indexOf(this.query.toLowerCase())) {beginswith.push(item);}
        else if (~item.indexOf(this.query)) {caseSensitive.push(item);}
        else {caseInsensitive.push(item);}
      }

      return beginswith.concat(caseSensitive, caseInsensitive);
    }

  , highlighter: function (item) {
      var query = this.query.replace(/[\-\[\]{}()*+?.,\\\^$|#\s]/g, '\\$&');
      return item.replace(new RegExp('(' + query + ')', 'ig'), function ($1, match) {
        return '<strong>' + match + '</strong>';
      })
    }

  , render: function (items) {
      var that = this;

      items = $(items).map(function (i, item) {
        i = $(that.options.item).attr('data-value', item);
        i.find('a').html(that.highlighter(item));
        return i[0];
      })

      items.first().addClass('active');
      this.$menu.html(items);
      return this;
    }

  , next: function (event) {
      var active = this.$menu.find('.active').removeClass('active')
        , next = active.next();

      if (!next.length) {
        next = $(this.$menu.find('li')[0]);
      }

      next.addClass('active');
    }

  , prev: function (event) {
      var active = this.$menu.find('.active').removeClass('active')
        , prev = active.prev();

      if (!prev.length) {
        prev = this.$menu.find('li').last();
      }

      prev.addClass('active');
    }

  , toggle: function () {
    if (!this.disabled) {
      //if (this.$container.hasClass('combobox-selected')) {
      //  this.clearTarget();
      //  this.triggerChange();
      //  this.clearElement();
      //} else {
        if (this.shown) {
          this.hide();
        } else {
          //this.clearElement();
          this.$element.focus();
          this.lookupAll();
        }
      //}
    }
  }

  , scrollSafety: function(e) {
      if (e.target.tagName == 'UL') {
          this.$element.off('blur');
      }
  }
  , clearElement: function () {
    this.$element.val('').focus();
  }

  , clearTarget: function () {
    this.$source.val('');
    this.$target.val('');
    this.$container.removeClass('combobox-selected');
    this.selected = false;
  }

  , triggerChange: function () {
    this.$source.trigger('change');
  }

  , refresh: function () {
    this.source = this.parse();
    this.options.items = this.source.length;
  }

  , listen: function () {
      this.$element
        .on('focus',    $.proxy(this.focus, this))
        .on('blur',     $.proxy(this.blur, this))
        .on('keypress', $.proxy(this.keypress, this))
        .on('keyup',    $.proxy(this.keyup, this));

      if (this.eventSupported('keydown')) {
        this.$element.on('keydown', $.proxy(this.keydown, this));
      }

      this.$menu
        .on('click', $.proxy(this.click, this))
        .on('mouseenter', 'li', $.proxy(this.mouseenter, this))
        .on('mouseleave', 'li', $.proxy(this.mouseleave, this));

      this.$button
        .on('click', $.proxy(this.toggle, this));
    }

  , eventSupported: function(eventName) {
      var isSupported = eventName in this.$element;
      if (!isSupported) {
        this.$element.setAttribute(eventName, 'return;');
        isSupported = typeof this.$element[eventName] === 'function';
      }
      return isSupported;
    }

  , move: function (e) {
      if (!this.shown) {return;}

      switch(e.keyCode) {
        case 9: // tab
        case 13: // enter
        case 27: // escape
          e.preventDefault();
          break;

        case 38: // up arrow
          e.preventDefault();
          this.prev();
          break;

        case 40: // down arrow
          e.preventDefault();
          this.next();
          break;
      }

      e.stopPropagation();
    }

  , keydown: function (e) {
      this.suppressKeyPressRepeat = ~$.inArray(e.keyCode, [40,38,9,13,27]);
      this.move(e);
    }

  , keypress: function (e) {
      if (this.suppressKeyPressRepeat) {return;}
      this.move(e);
    }

  , keyup: function (e) {
      switch(e.keyCode) {
        case 40: // down arrow
        case 39: // right arrow
        case 38: // up arrow
        case 37: // left arrow
        case 36: // home
        case 35: // end
        case 16: // shift
        case 17: // ctrl
        case 18: // alt
          break;

        case 9: // tab
        case 13: // enter
          if (!this.shown) {return;}
          this.select();
          break;

        case 27: // escape
          if (!this.shown) {return;}
          this.hide();
          break;

        default:
          this.clearTarget();
          this.lookup();
      }

      e.stopPropagation();
      e.preventDefault();
  }

  , focus: function (e) {
      this.focused = true;
    }

  , blur: function (e) {
      var that = this;
      this.focused = false;
      var val = this.$element.val();
      if (!this.selected && val !== '' ) {
        this.$element.val('');
        this.$source.val('').trigger('change');
        this.$target.val('').trigger('change');
      }
      if (!this.mousedover && this.shown) {setTimeout(function () { that.hide(); }, 200);}
    }

  , click: function (e) {
      e.stopPropagation();
      e.preventDefault();
      this.select();
      this.$element.focus();
    }

  , mouseenter: function (e) {
      this.mousedover = true;
      this.$menu.find('.active').removeClass('active');
      $(e.currentTarget).addClass('active');
    }

  , mouseleave: function (e) {
      this.mousedover = false;
    }
  };

  /* COMBOBOX PLUGIN DEFINITION
   * =========================== */

  $.fn.combobox = function ( option ) {
    return this.each(function () {
      var $this = $(this)
        , data = $this.data('combobox')
        , options = typeof option == 'object' && option;
      if(!data) {$this.data('combobox', (data = new Combobox(this, options)));}
      if (typeof option == 'string') {data[option]();}
    });
  };

  $.fn.combobox.defaults = {
    bsVersion: '3'
  , menu: '<ul class="typeahead typeahead-long dropdown-menu"></ul>'
  , item: '<li><a href="#"></a></li>'
  };

  $.fn.combobox.Constructor = Combobox;

}( window.jQuery );

// Init
minScreen = 767;
if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
    $('.combobox').combobox();
}

var selectedDistrictId;
var selectedWardId;
var selectedStreetId;

$("select[id$=ProvinceId]").change(function () {
    if ($(this).val() > 0) {
        var slcDistrict = $("#" + this.id.replace("ProvinceId", "DistrictId"), $(this).closest('form'));
        if (slcDistrict.length > 0) {
            slcDistrict.siblings('.combobox-container').children('input').val('');
            slcDistrict.change();
            var dataToPost = {
                provinceId: $(this).val()
            };
            if ($(this).hasClass('restricted')) {
                dataToPost.userId = $(this).attr('userid');
            }
            $.ajax({
                type: "get",
                dataType: "",
                url: "/RealEstate.Admin/Home/GetDistrictsForJson",
                data: dataToPost,
                success: function (response) {
                    var selectedValues = slcDistrict.val();
                    slcDistrict.empty().append("<option value=''>--Quận / Huyện--</option>");
                    $.each(response.list, function (i, item) { slcDistrict.append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
                    if (selectedDistrictId != null) slcDistrict.val(selectedDistrictId);
                    else slcDistrict.val(selectedValues);
                    slcDistrict.change();
                    if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                        slcDistrict.combobox('refresh');
                    }
                    $('#WardIds, #StreetIds, #DirectionIds').closest('.form-group').hide();
                },
                error: function (request, status, error) {
                    slcDistrict.empty().append("<option value=''></option>").change();
                    if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                        slcDistrict.combobox('refresh');
                    }
                }
            });
        }
    }
    return false;
});
$("select[id$=DistrictId]").change(function () {
    var slcWard = $("#" + this.id.replace("DistrictId", "WardId"), $(this).closest('form'));
    var slcStreet = $("#" + this.id.replace("DistrictId", "StreetId"), $(this).closest('form'));
    var slcApartment = $("#" + this.id.replace("DistrictId", "ApartmentId"), $(this).closest('form'));

    if ($(this).val()) {
        // Ward
        if (slcWard.length > 0) {
            slcWard.children("option:first").text("[Loading..]");
            slcWard.siblings('.combobox-container').children('input').val('');
            $.ajax({
                type: "get",
                dataType: "",
                url: "/RealEstate.Admin/Home/GetWardsForJson",
                data: {
                    districtId: $(this).val()
                },
                success: function (response) {
                    var selectedValues = slcWard.val();
                    slcWard.empty().append("<option value=''></option>");
                    $.each(response.list, function (i, item) { slcWard.append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
                    if (selectedWardId != null) slcWard.val(selectedWardId);
                    else slcWard.val(selectedValues);
                    slcWard.change();
                    if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                        slcWard.combobox('refresh');
                    }
                },
                error: function (request, status, error) {
                    slcWard.empty().append("<option value=''></option>").change();
                    if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                        slcWard.combobox('refresh');
                    }
                }
            });
        }

        // Street
        if (slcStreet.length > 0) {
            slcStreet.children("option:first").text("[Loading..]");
            slcStreet.siblings('.combobox-container').children('input').val('');
            var _url = "/RealEstate.Admin/Home/GetStreetsForJson";
            if (slcStreet.hasClass('all-street'))
                _url = "/RealEstate.Admin/Home/GetAllStreetsForJson";
            $.ajax({
                type: "get",
                dataType: "",
                url: _url,
                data: {
                    districtId: $(this).val()
                },
                success: function (response) {
                    var selectedValues = slcStreet.val();
                    slcStreet.empty().append("<option value=''></option>");
                    $.each(response.list, function (i, item) { slcStreet.append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
                    if (selectedStreetId != null) slcStreet.val(selectedStreetId);
                    else slcStreet.val(selectedValues);
                    slcStreet.change();
                    if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                        slcStreet.combobox('refresh');
                    }
                },
                error: function (request, status, error) {
                    slcStreet.empty().append("<option value=''></option>").change();
                    if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                        slcStreet.combobox('refresh');
                    }
                }
            });
        }

        // Apartment
        if (slcApartment.length > 0) {
            slcApartment.children("option:first").text("[Loading..]");
            slcApartment.siblings('.combobox-container').children('input').val('');
            $.ajax({
                type: "get",
                dataType: "",
                url: "/RealEstate.Admin/Home/GetApartmentsForJson",
                data: {
                    districtId: $(this).val()
                },
                success: function (response) {
                    slcApartment.empty().append("<option value=''></option>");
                    $.each(response.list, function (i, item) { slcApartment.append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
                    slcApartment.change();
                    if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                        slcApartment.combobox('refresh');
                    }
                },
                error: function (request, status, error) {
                    slcApartment.empty().append("<option value=''></option>").change();
                    if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                        slcApartment.combobox('refresh');
                    }
                }
            });
        }

        
    }
    else {
        // Ward
        if (slcWard.length > 0) {
            slcWard.empty().append("<option value=''></option>").change();
            if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                slcWard.combobox('refresh');
            }
        }

        // Street
        if (slcStreet.length > 0) {
            slcStreet.empty().append("<option value=''></option>").change();
            if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                slcStreet.combobox('refresh');
            }
        }

        // Apartment
        if (slcApartment.length > 0) {
            slcApartment.empty().append("<option value=''></option>").change();
            if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                slcApartment.combobox('refresh');
            }
        }

        
    }
    return false;
});

$("select[id$=ApartmentId]").change(function () {
    var slcApartmentBlock = $("#" + this.id.replace("ApartmentId", "ApartmentBlockId"), $(this).closest('form'));
    var oldApartmentBlockId = $('#oldApartmentBlockId').val();

    if ($(this).val()) {
        //ApartmentBlock
        if (slcApartmentBlock.length > 0) {
            slcApartmentBlock.children("option:first").text("[Loading..]");
            slcApartmentBlock.siblings('.combobox-container').children('input').val('');
            $.ajax({
                type: "get",
                dataType: "",
                url: "/RealEstate.Admin/Home/GetApartmentBlocksForJson",
                data: {
                    apartmentId: $(this).val()
                },
                success: function(response) {
                    slcApartmentBlock.empty().append("<option value=''>-- Vui lòng chọn --</option>");
                    $.each(response.list, function (i, item) { slcApartmentBlock.append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
                    slcApartmentBlock.val(oldApartmentBlockId);
                    slcApartmentBlock.change();
                    if ($(window).width() > minScreen) { // kiem tra kich thuoc man hinh cua thiet bi
                        slcApartmentBlock.combobox('refresh');
                    }
                },
                error: function(request, status, error) {
                    slcApartmentBlock.empty().append("<option value=''>-- Vui lòng chọn --</option>").change();
                    if ($(window).width() > minScreen) { // kiem tra kich thuoc man hinh cua thiet bi
                        slcApartmentBlock.combobox('refresh');
                    }
                }
            });
        }
    } else {
        // ApartmentBlock
        if (slcApartmentBlock.length > 0) {
            slcApartmentBlock.empty().append("<option value=''>-- Vui lòng chọn --</option>").change();
            if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                slcApartmentBlock.combobox('refresh');
            }
        }
    }
    return false;
});

//find map location
$('#WardId, #StreetId, #AddressNumber, #AddressCorner').change(function () {
    if (typeof geocoder != 'undefined') {
        var province = $('#ProvinceId option:selected').text();
        var district = $('#DistrictId option:selected').text();
        var ward = $('#WardId option:selected').text();
        var street = $('#StreetId option:selected').text();
        var addressnumber = $('#AddressNumber').val();
        var addresscorner = $('#AddressCorner').val();
        
        if (province != '') $('#address').val(province);
        if (district != '' && district != '-- Vui lòng chọn --') $('#address').val(district + ', ' + $('#address').val());
        if (ward != '' && ward != '-- Vui lòng chọn --' && ward != '[Loading..]') $('#address').val(ward.split('-')[0] + ', ' + $('#address').val());
        if (street != '' && street != '-- Vui lòng chọn --') $('#address').val(street.split('-')[0] + ', ' + $('#address').val());

        if (addressnumber != '') {
            addressnumber = addressnumber.split('/')[0];
            $('#address').val(addressnumber + ' , ' + $('#address').val());
        }
        //console.log($('#address').val());
        // Find location on Map
        geocode();
    }
});
//if ($('#ProvinceId option').length == 1) disableCombobox($("#ProvinceId"));
//if ($('#DistrictId option').length == 1) disableCombobox($("#DistrictId"));
//if ($('#WardId option').length == 1) disableCombobox($("#WardId"));
//if ($('#StreetId option').length == 1) disableCombobox($("#StreetId"));
