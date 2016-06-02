/**
 * bootstrap-multiselect.js 1.0.0
 * https://github.com/davidstutz/bootstrap-multiselect
 *
 * Copyright 2012, 2013 David Stutz
 * 
 * Dual licensed under the BSD-3-Clause and the Apache License, Version 2.0.
 * See the README.
 */
!function ($) {
    "use strict"; // jshint ;_;

    if (typeof ko != 'undefined' && ko.bindingHandlers && !ko.bindingHandlers.multiselect) {
        ko.bindingHandlers.multiselect = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            },
            update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var ms = $(element).data('multiselect');
                if (!ms) {
                    $(element).multiselect(ko.utils.unwrapObservable(valueAccessor()));
                }
                else
                    if (allBindingsAccessor().options && allBindingsAccessor().options().length !== ms.originalOptions.length) {
                        ms.updateOriginalOptions();
                        $(element).multiselect('rebuild');
                    }
            }
        };
    }

    function Multiselect(select, options) {

        this.options = this.getOptions(options);
        this.$select = $(select);
        this.originalOptions = this.$select.clone()[0].options;
        //we have to clone to create a new reference
        this.query = '';
        this.searchTimeout = null;

        this.options.multiple = this.$select.attr('multiple') == "multiple";

        this.$container = $(this.options.buttonContainer).append('<button type="button" class="multiselect dropdown-toggle ' + this.options.buttonClass(this.getSelected(), this.$select) + '" data-toggle="dropdown">' + this.options.buttonText(this.getSelected(), this.$select) + '</button>')
            .append('<ul class="multiselect-container dropdown-menu' + (this.options.dropRight ? ' pull-right' : '') + '"></ul>');

        // Manually add button width if set.
        if (this.options.buttonWidth) {
            $('button', this.$container).css({
                'width': this.options.buttonWidth
            });
        }

        // Keep the tab index from the select.
        var tabindex = this.$select.attr('tabindex');
        if (tabindex) {
            $('button', this.$container).attr('tabindex', tabindex);
        }

        // Set max height of dropdown menu to activate auto scrollbar.
        if (this.options.maxHeight) {
            // TODO: Add a class for this option to move the css declarations.
            $('.multiselect-container', this.$container).css({
                'max-height': this.options.maxHeight + 'px',
                'overflow-y': 'auto',
                'overflow-x': 'hidden'
            });
        }

        // Enable filtering.
        if (this.options.enableFiltering || this.options.enableCaseInsensitiveFiltering) {
            var enableFilterLength = Math.max(this.options.enableFiltering, this.options.enableCaseInsensitiveFiltering);
            if (this.$select.find('option').length >= enableFilterLength) {
                this.buildFilter();
            }
        }

        // Build select all if enabled.
        this.buildSelectAll();
        this.buildDropdown();
        this.updateButtonText();

        this.$select.hide().after(this.$container);
    };

    Multiselect.prototype = {

        defaults: {
            // Default text function will either print 'None selected' in case no
            // option is selected, or a list of the selected options up to a length of 3 selected options.
            // If more than 3 options are selected, the number of selected options is printed.
            buttonText: function (options, select) {
                if (options.length == 0) {
                    return this.nonSelectedText + '<b class="caret"></b>';
                }
                else
                    if (options.length > 3) {
                        return options.length + ' ' + this.nSelectedText + ' <b class="caret"></b>';
                    }
                    else {
                        var selected = '';
                        options.each(function () {
                            var label = ($(this).attr('label') !== undefined) ? $(this).attr('label') : $(this).html();

                            selected += label + ', ';
                        });
                        return selected.substr(0, selected.length - 2) + ' <b class="caret"></b>';
                    }
            },
            // Like the buttonText option to update the title of the button.
            buttonTitle: function (options, select) {
                var selected = '';
                options.each(function () {
                    selected += $(this).text() + ', ';
                });
                return selected.substr(0, selected.length - 2);
            },
            // Is triggered on change of the selected options.
            onChange: function (option, checked) {

            },
            buttonClass: function (options, select) { return 'btn'; },
            dropRight: false,
            selectedClass: 'active',
            buttonWidth: 'auto',
            buttonContainer: '<div class="btn-group" />',
            // Maximum height of the dropdown menu.
            // If maximum height is exceeded a scrollbar will be displayed.
            maxHeight: false,
            includeSelectAllOption: false,
            selectAllText: ' Select all',
            selectAllValue: 'multiselect-all',
            enableFiltering: false,
            enableCaseInsensitiveFiltering: false,
            filterPlaceholder: 'Search',
            // possible options: 'text', 'value', 'both'
            filterBehavior: 'text',
            preventInputChangeEvent: false,
            nonSelectedText: 'None selected',
            nSelectedText: 'selected'
        },

        constructor: Multiselect,

        // Will build an dropdown element for the given option.
        createOptionValue: function (element) {
            if ($(element).is(':selected')) {
                $(element).attr('selected', 'selected').prop('selected', true);
            }

            // Support the label attribute on options.
            var label = $(element).attr('label') || $(element).html();
            var value = $(element).val();
            var inputType = this.options.multiple ? "checkbox" : "radio";

            var $li = $('<li><a href="javascript:void(0);"><label class="' + inputType + '"><input type="' + inputType + '" /></label></a></li>');

            var selected = $(element).prop('selected') || false;
            var $checkbox = $('input', $li);
            $checkbox.val(value);

            if (value == this.options.selectAllValue) {
                $checkbox.parent().parent().addClass('multiselect-all');
            }

            $('label', $li).append(" " + label);

            $('.multiselect-container', this.$container).append($li);

            if ($(element).is(':disabled')) {
                $checkbox.attr('disabled', 'disabled').prop('disabled', true).parents('li').addClass('disabled');
            }

            $checkbox.prop('checked', selected);

            if (selected && this.options.selectedClass) {
                $checkbox.parents('li').addClass(this.options.selectedClass);
            }
        },

        toggleActiveState: function (shouldBeActive) {
            if (this.$select.attr('disabled') == undefined) {
                $('button.multiselect.dropdown-toggle', this.$container).removeClass('disabled');
            }
            else {
                $('button.multiselect.dropdown-toggle', this.$container).addClass('disabled');
            }
        },

        // Add the select all option to the select.
        buildSelectAll: function () {
            var alreadyHasSelectAll = this.$select[0][0] ? this.$select[0][0].value == this.options.selectAllValue : false;

            // If options.includeSelectAllOption === true, add the include all checkbox.
            if (this.options.includeSelectAllOption && this.options.multiple && !alreadyHasSelectAll) {
                this.$select.prepend('<option value="' + this.options.selectAllValue + '">' + this.options.selectAllText + '</option>');
            }
        },

        // Build the dropdown and bind event handling.
        buildDropdown: function () {
            this.toggleActiveState();

            this.$select.children().each($.proxy(function (index, element) {
                // Support optgroups and options without a group simultaneously.
                var tag = $(element).prop('tagName').toLowerCase();
                if (tag == 'optgroup') {
                    var group = element;
                    var groupName = $(group).prop('label');

                    // Add a header for the group.
                    var $li = $('<li><label class="multiselect-group"></label></li>');
                    $('label', $li).text(groupName);
                    $('.multiselect-container', this.$container).append($li);

                    // Add the options of the group.
                    $('option', group).each($.proxy(function (index, element) {
                        this.createOptionValue(element);
                    }, this));
                }
                else
                    if (tag == 'option') {
                        this.createOptionValue(element);
                    }
                    else {
                        // Ignore illegal tags.
                    }
            }, this));

            // Bind the change event on the dropdown elements.
            $('.multiselect-container li input', this.$container).on('change', $.proxy(function (event) {
                var checked = $(event.target).prop('checked') || false;
                var isSelectAllOption = $(event.target).val() == this.options.selectAllValue;

                // Apply or unapply the configured selected class.
                if (this.options.selectedClass) {
                    if (checked) {
                        $(event.target).parents('li').addClass(this.options.selectedClass);
                    }
                    else {
                        $(event.target).parents('li').removeClass(this.options.selectedClass);
                    }
                }

                var $option = $('option', this.$select).filter(function () {
                    return $(this).val() == $(event.target).val();
                });

                var $optionsNotThis = $('option', this.$select).not($option);
                var $checkboxesNotThis = $('input', this.$container).not($(event.target));

                // Toggle all options if the select all option was changed.
                if (isSelectAllOption) {
                    $checkboxesNotThis.filter(function () {
                        return $(this).is(':checked') != checked;
                    }).trigger('click');
                }

                if (checked) {
                    $option.prop('selected', true);

                    if (this.options.multiple) {
                        $option.attr('selected', 'selected');
                    }
                    else {
                        if (this.options.selectedClass) {
                            $($checkboxesNotThis).parents('li').removeClass(this.options.selectedClass);
                        }

                        $($checkboxesNotThis).prop('checked', false);

                        $optionsNotThis.removeAttr('selected').prop('selected', false);

                        // It's a single selection, so close.
                        $(this.$container).find(".multiselect.dropdown-toggle").click();
                    }

                    if (this.options.selectedClass == "active") {
                        $optionsNotThis.parents("a").css("outline", "");
                    }

                }
                else {
                    $option.removeAttr('selected').prop('selected', false);
                }

                this.updateButtonText();

                this.options.onChange($option, checked);

                this.$select.change();

                if (this.options.preventInputChangeEvent) {
                    return false;
                }
            }, this));

            $('.multiselect-container li a', this.$container).on('touchstart click', function (event) {
                event.stopPropagation();
                $(event.target).blur();
            });

            // Keyboard support.
            this.$container.on('keydown', $.proxy(function (event) {
                if ($('input[type="text"]', this.$container).is(':focus'))
                    return;
                if ((event.keyCode == 9 || event.keyCode == 27) && this.$container.hasClass('open')) {
                    // Close on tab or escape.
                    $(this.$container).find(".multiselect.dropdown-toggle").click();
                }
                else {
                    var $items = $(this.$container).find("li:not(.divider):visible a");

                    if (!$items.length) {
                        return;
                    }

                    var index = $items.index($items.filter(':focus'));

                    // Navigation up.
                    if (event.keyCode == 38 && index > 0) {
                        index--;
                    }
                    // Navigate down.
                    else
                        if (event.keyCode == 40 && index < $items.length - 1) {
                            index++;
                        }
                        else
                            if (! ~index) {
                                index = 0;
                            }

                    var $current = $items.eq(index);
                    $current.focus();

                    // Override style for items in li:active.
                    if (this.options.selectedClass == "active") {
                        $current.css("outline", "thin dotted #333").css("outline", "5px auto -webkit-focus-ring-color");

                        $items.not($current).css("outline", "");
                    }

                    if (event.keyCode == 32 || event.keyCode == 13) {
                        var $checkbox = $current.find('input');

                        $checkbox.prop("checked", !$checkbox.prop("checked"));
                        $checkbox.change();
                    }

                    event.stopPropagation();
                    event.preventDefault();
                }
            }, this));
        },

        // Build and bind filter.
        buildFilter: function () {
            $('.multiselect-container', this.$container).prepend('<div class="input-group"><span class="input-group-addon"><span class="glyphicon glyphicon-search"></span></span><input class="multiselect-search form-control input-sm" type="text" placeholder="' + this.options.filterPlaceholder + '"></div>');

            $('.multiselect-search', this.$container).val(this.query).on('click', function (event) {
                event.stopPropagation();
            }).on('keydown', $.proxy(function (event) {
                // This is useful to catch "keydown" events after the browser has
                // updated the control.
                clearTimeout(this.searchTimeout);

                this.searchTimeout = this.asyncFunction($.proxy(function () {

                    if (this.query != event.target.value) {
                        this.query = event.target.value;

                        $.each($('.multiselect-container li', this.$container), $.proxy(function (index, element) {
                            var value = $('input', element).val();
                            if (value != this.options.selectAllValue) {
                                var text = $('label', element).text();
                                var value = $('input', element).val();
                                if (value && text && value != this.options.selectAllValue) {
                                    // by default lets assume that element is not
                                    // interesting for this search
                                    var showElement = false;

                                    var filterCandidate = '';
                                    if ((this.options.filterBehavior == 'text' || this.options.filterBehavior == 'both')) {
                                        filterCandidate = text;
                                    }
                                    if ((this.options.filterBehavior == 'value' || this.options.filterBehavior == 'both')) {
                                        filterCandidate = value;
                                    }

                                    if (this.options.enableCaseInsensitiveFiltering && filterCandidate.toLowerCase().indexOf(this.query.toLowerCase()) > -1) {
                                        showElement = true;
                                    }
                                    else if (filterCandidate.indexOf(this.query) > -1) {
                                        showElement = true;
                                    }

                                    if (showElement) {
                                        $(element).show();
                                    }
                                    else {
                                        $(element).hide();
                                    }
                                }
                            }
                        }, this));
                    }
                }, this), 300, this);
            }, this));
        },

        // Destroy - unbind - the plugin.
        destroy: function () {
            this.$container.remove();
            this.$select.show();
        },

        // Refreshs the checked options based on the current state of the select.
        refresh: function () {
            $('option', this.$select).each($.proxy(function (index, element) {
                var $input = $('.multiselect-container li input', this.$container).filter(function () {
                    return $(this).val() == $(element).val();
                });

                if ($(element).is(':selected')) {
                    $input.prop('checked', true);

                    if (this.options.selectedClass) {
                        $input.parents('li').addClass(this.options.selectedClass);
                    }
                }
                else {
                    $input.prop('checked', false);

                    if (this.options.selectedClass) {
                        $input.parents('li').removeClass(this.options.selectedClass);
                    }
                }

                if ($(element).is(":disabled")) {
                    $input.attr('disabled', 'disabled').prop('disabled', true).parents('li').addClass('disabled');
                }
                else {
                    $input.removeAttr('disabled').prop('disabled', false).parents('li').removeClass('disabled');
                }
            }, this));

            this.updateButtonText();
        },

        // Select an option by its value.
        select: function (value) {
            var $option = $('option', this.$select).filter(function () {
                return $(this).val() == value;
            });
            var $checkbox = $('.multiselect-container li input', this.$container).filter(function () {
                return $(this).val() == value;
            });

            if (this.options.selectedClass) {
                $checkbox.parents('li').addClass(this.options.selectedClass);
            }

            $checkbox.prop('checked', true);

            $option.attr('selected', 'selected').prop('selected', true);

            this.updateButtonText();
            this.options.onChange($option, true);
        },

        // Deselect an option by its value.
        deselect: function (value) {
            var $option = $('option', this.$select).filter(function () {
                return $(this).val() == value;
            });
            var $checkbox = $('.multiselect-container li input', this.$container).filter(function () {
                return $(this).val() == value;
            });

            if (this.options.selectedClass) {
                $checkbox.parents('li').removeClass(this.options.selectedClass);
            }

            $checkbox.prop('checked', false);

            $option.removeAttr('selected').prop('selected', false);

            this.updateButtonText();
            this.options.onChange($option, false);
        },

        // Rebuild the whole dropdown menu.
        rebuild: function () {
            $('.multiselect-container', this.$container).html('');

            this.buildSelectAll();
            this.buildDropdown(this.$select, this.options);
            this.updateButtonText();

            // Enable filtering.
            if (this.options.enableFiltering || this.options.enableCaseInsensitiveFiltering) {
                this.buildFilter();
            }
        },

        // Get options by merging defaults and given options.
        getOptions: function (options) {
            return $.extend({}, this.defaults, options);
        },

        updateButtonText: function () {
            var options = this.getSelected();

            // First update the displayed button text.
            $('button', this.$container).html(this.options.buttonText(options, this.$select));

            // Now update the title attribute of the button.
            $('button', this.$container).attr('title', this.options.buttonTitle(options, this.$select));

        },

        // Get all selected options.
        getSelected: function () {
            return $('option:selected[value!="' + this.options.selectAllValue + '"]', this.$select);
        },

        updateOriginalOptions: function () {
            this.originalOptions = this.$select.clone()[0].options;
        },

        asyncFunction: function (callback, timeout, self) {
            var args = Array.prototype.slice.call(arguments, 3);
            return setTimeout(function () {
                callback.apply(self || window, args);
            }, timeout);
        }
    };

    $.fn.multiselect = function (option, parameter) {
        return this.each(function () {
            var data = $(this).data('multiselect'), options = typeof option == 'object' && option;

            // Initialize the multiselect.
            if (!data) {
                $(this).data('multiselect', (data = new Multiselect(this, options)));
            }

            // Call multiselect method.
            if (typeof option == 'string') {
                data[option](parameter);
            }
        });
    };

    $.fn.multiselect.Constructor = Multiselect;

    $(function () {
        $("select[data-role=multiselect]").multiselect();
    });

}(window.jQuery);

//Bootstrap-select-picker
;!function (b) { b.expr[":"].icontains = function (e, c, d) { return b(e).text().toUpperCase().indexOf(d[3].toUpperCase()) >= 0 }; var a = function (d, c, f) { if (f) { f.stopPropagation(); f.preventDefault() } this.$element = b(d); this.$newElement = null; this.$button = null; this.$menu = null; this.$lis = null; this.options = b.extend({}, b.fn.selectpicker.defaults, this.$element.data(), typeof c == "object" && c); if (this.options.title === null) { this.options.title = this.$element.attr("title") } this.val = a.prototype.val; this.render = a.prototype.render; this.refresh = a.prototype.refresh; this.setStyle = a.prototype.setStyle; this.selectAll = a.prototype.selectAll; this.deselectAll = a.prototype.deselectAll; this.init() }; a.prototype = { constructor: a, init: function () { var c = this, d = this.$element.attr("id"); this.$element.hide(); this.multiple = this.$element.prop("multiple"); this.autofocus = this.$element.prop("autofocus"); this.$newElement = this.createView(); this.$element.after(this.$newElement); this.$menu = this.$newElement.find("> .dropdown-menu"); this.$button = this.$newElement.find("> button"); this.$searchbox = this.$newElement.find("input"); if (d !== undefined) { this.$button.attr("data-id", d); b('label[for="' + d + '"]').click(function (f) { f.preventDefault(); c.$button.focus() }) } this.checkDisabled(); this.clickListener(); if (this.options.liveSearch) { this.liveSearchListener() } this.render(); this.liHeight(); this.setStyle(); this.setWidth(); if (this.options.container) { this.selectPosition() } this.$menu.data("this", this); this.$newElement.data("this", this) }, createDropdown: function () { var c = this.multiple ? " show-tick" : ""; var d = this.$element.parent().hasClass("input-group") ? " input-group-btn" : ""; var i = this.autofocus ? " autofocus" : ""; var h = this.options.header ? '<div class="popover-title"><button type="button" class="close" aria-hidden="true">&times;</button>' + this.options.header + "</div>" : ""; var g = this.options.liveSearch ? '<div class="bootstrap-select-searchbox"><input type="text" class="input-block-level form-control" /></div>' : ""; var f = this.options.actionsBox ? '<div class="bs-actionsbox"><div class="btn-group btn-block"><button class="actions-btn bs-select-all btn btn-sm btn-default">Select All</button><button class="actions-btn bs-deselect-all btn btn-sm btn-default">Deselect All</button></div></div>' : ""; var e = '<div class="btn-group bootstrap-select' + c + d + '"><button type="button" class="btn dropdown-toggle selectpicker" data-toggle="dropdown"' + i + '><span class="filter-option pull-left"></span>&nbsp;<span class="caret"></span></button><div class="dropdown-menu open">' + h + g + f + '<ul class="dropdown-menu inner selectpicker" role="menu"></ul></div></div>'; return b(e) }, createView: function () { var c = this.createDropdown(); var d = this.createLi(); c.find("ul").append(d); return c }, reloadLi: function () { this.destroyLi(); var c = this.createLi(); this.$menu.find("ul").append(c) }, destroyLi: function () { this.$menu.find("li").remove() }, createLi: function () { var d = this, e = [], c = ""; this.$element.find("option").each(function () { var i = b(this); var g = i.attr("class") || ""; var h = i.attr("style") || ""; var m = i.data("content") ? i.data("content") : i.html(); var k = i.data("subtext") !== undefined ? '<small class="muted text-muted">' + i.data("subtext") + "</small>" : ""; var j = i.data("icon") !== undefined ? '<i class="' + d.options.iconBase + " " + i.data("icon") + '"></i> ' : ""; if (j !== "" && (i.is(":disabled") || i.parent().is(":disabled"))) { j = "<span>" + j + "</span>" } if (!i.data("content")) { m = j + '<span class="text">' + m + k + "</span>" } if (d.options.hideDisabled && (i.is(":disabled") || i.parent().is(":disabled"))) { e.push('<a style="min-height: 0; padding: 0"></a>') } else { if (i.parent().is("optgroup") && i.data("divider") !== true) { if (i.index() === 0) { var l = i.parent().attr("label"); var n = i.parent().data("subtext") !== undefined ? '<small class="muted text-muted">' + i.parent().data("subtext") + "</small>" : ""; var f = i.parent().data("icon") ? '<i class="' + i.parent().data("icon") + '"></i> ' : ""; l = f + '<span class="text">' + l + n + "</span>"; if (i[0].index !== 0) { e.push('<div class="div-contain"><div class="divider"></div></div><dt>' + l + "</dt>" + d.createA(m, "opt " + g, h)) } else { e.push("<dt>" + l + "</dt>" + d.createA(m, "opt " + g, h)) } } else { e.push(d.createA(m, "opt " + g, h)) } } else { if (i.data("divider") === true) { e.push('<div class="div-contain"><div class="divider"></div></div>') } else { if (b(this).data("hidden") === true) { e.push("<a></a>") } else { e.push(d.createA(m, g, h)) } } } } }); b.each(e, function (g, h) { var f = h === "<a></a>" ? 'class="hide is-hidden"' : ""; c += '<li rel="' + g + '"' + f + ">" + h + "</li>" }); if (!this.multiple && this.$element.find("option:selected").length === 0 && !this.options.title) { this.$element.find("option").eq(0).prop("selected", true).attr("selected", "selected") } return b(c) }, createA: function (e, c, d) { return '<a tabindex="0" class="' + c + '" style="' + d + '">' + e + '<i class="' + this.options.iconBase + " " + this.options.tickIcon + ' icon-ok check-mark"></i></a>' }, render: function (e) { var d = this; if (e !== false) { this.$element.find("option").each(function (i) { d.setDisabled(i, b(this).is(":disabled") || b(this).parent().is(":disabled")); d.setSelected(i, b(this).is(":selected")) }) } this.tabIndex(); var h = this.$element.find("option:selected").map(function () { var k = b(this); var j = k.data("icon") && d.options.showIcon ? '<i class="' + d.options.iconBase + " " + k.data("icon") + '"></i> ' : ""; var i; if (d.options.showSubtext && k.attr("data-subtext") && !d.multiple) { i = ' <small class="muted text-muted">' + k.data("subtext") + "</small>" } else { i = "" } if (k.data("content") && d.options.showContent) { return k.data("content") } else { if (k.attr("title") !== undefined) { return k.attr("title") } else { return j + k.html() + i } } }).toArray(); var g = !this.multiple ? h[0] : h.join(this.options.multipleSeparator); if (this.multiple && this.options.selectedTextFormat.indexOf("count") > -1) { var c = this.options.selectedTextFormat.split(">"); var f = this.options.hideDisabled ? ":not([disabled])" : ""; if ((c.length > 1 && h.length > c[1]) || (c.length == 1 && h.length >= 2)) { g = this.options.countSelectedText.replace("{0}", h.length).replace("{1}", this.$element.find('option:not([data-divider="true"]):not([data-hidden="true"])' + f).length) } } this.options.title = this.$element.attr("title"); if (!g) { g = this.options.title !== undefined ? this.options.title : this.options.noneSelectedText } this.$button.attr("title", b.trim(g)); this.$newElement.find(".filter-option").html(g) }, setStyle: function (e, d) { if (this.$element.attr("class")) { this.$newElement.addClass(this.$element.attr("class").replace(/selectpicker|mobile-device/gi, "")) } var c = e ? e : this.options.style; if (d == "add") { this.$button.addClass(c) } else { if (d == "remove") { this.$button.removeClass(c) } else { this.$button.removeClass(this.options.style); this.$button.addClass(c) } } }, liHeight: function () { if (this.options.size === false) { return } var f = this.$menu.parent().clone().find("> .dropdown-toggle").prop("autofocus", false).end().appendTo("body"), g = f.addClass("open").find("> .dropdown-menu"), e = g.find("li > a").outerHeight(), d = this.options.header ? g.find(".popover-title").outerHeight() : 0, h = this.options.liveSearch ? g.find(".bootstrap-select-searchbox").outerHeight() : 0, c = this.options.actionsBox ? g.find(".bs-actionsbox").outerHeight() : 0; f.remove(); this.$newElement.data("liHeight", e).data("headerHeight", d).data("searchHeight", h).data("actionsHeight", c) }, setSize: function () { var i = this, d = this.$menu, j = d.find(".inner"), u = this.$newElement.outerHeight(), f = this.$newElement.data("liHeight"), s = this.$newElement.data("headerHeight"), m = this.$newElement.data("searchHeight"), h = this.$newElement.data("actionsHeight"), l = d.find("li .divider").outerHeight(true), r = parseInt(d.css("padding-top")) + parseInt(d.css("padding-bottom")) + parseInt(d.css("border-top-width")) + parseInt(d.css("border-bottom-width")), p = this.options.hideDisabled ? ":not(.disabled)" : "", o = b(window), g = r + parseInt(d.css("margin-top")) + parseInt(d.css("margin-bottom")) + 2, q, v, t, k = function () { v = i.$newElement.offset().top - o.scrollTop(); t = o.height() - v - u }; k(); if (this.options.header) { d.css("padding-top", 0) } if (this.options.size == "auto") { var e = function () { var x, w = i.$lis.not(".hide"); k(); q = t - g; if (i.options.dropupAuto) { i.$newElement.toggleClass("dropup", (v > t) && ((q - g) < d.height())) } if (i.$newElement.hasClass("dropup")) { q = v - g } if ((w.length + w.find("dt").length) > 3) { x = f * 3 + g - 2 } else { x = 0 } d.css({ "max-height": q + "px", overflow: "hidden", "min-height": x + s + m + h + "px" }); j.css({ "max-height": q - s - m - h - r + "px", "overflow-y": "auto", "min-height": Math.max(x - r, 0) + "px" }) }; e(); this.$searchbox.off("input.getSize propertychange.getSize").on("input.getSize propertychange.getSize", e); b(window).off("resize.getSize").on("resize.getSize", e); b(window).off("scroll.getSize").on("scroll.getSize", e) } else { if (this.options.size && this.options.size != "auto" && d.find("li" + p).length > this.options.size) { var n = d.find("li" + p + " > *").filter(":not(.div-contain)").slice(0, this.options.size).last().parent().index(); var c = d.find("li").slice(0, n + 1).find(".div-contain").length; q = f * this.options.size + c * l + r; if (i.options.dropupAuto) { this.$newElement.toggleClass("dropup", (v > t) && (q < d.height())) } d.css({ "max-height": q + s + m + h + "px", overflow: "hidden" }); j.css({ "max-height": q - r + "px", "overflow-y": "auto" }) } } }, setWidth: function () { if (this.options.width == "auto") { this.$menu.css("min-width", "0"); var e = this.$newElement.clone().appendTo("body"); var c = e.find("> .dropdown-menu").css("width"); var d = e.css("width", "auto").find("> button").css("width"); e.remove(); this.$newElement.css("width", Math.max(parseInt(c), parseInt(d)) + "px") } else { if (this.options.width == "fit") { this.$menu.css("min-width", ""); this.$newElement.css("width", "").addClass("fit-width") } else { if (this.options.width) { this.$menu.css("min-width", ""); this.$newElement.css("width", this.options.width) } else { this.$menu.css("min-width", ""); this.$newElement.css("width", "") } } } if (this.$newElement.hasClass("fit-width") && this.options.width !== "fit") { this.$newElement.removeClass("fit-width") } }, selectPosition: function () { var e = this, d = "<div />", f = b(d), h, g, c = function (i) { f.addClass(i.attr("class").replace(/form-control/gi, "")).toggleClass("dropup", i.hasClass("dropup")); h = i.offset(); g = i.hasClass("dropup") ? 0 : i[0].offsetHeight; f.css({ top: h.top + g, left: h.left, width: i[0].offsetWidth, position: "absolute" }) }; this.$newElement.on("click", function () { if (e.isDisabled()) { return } c(b(this)); f.appendTo(e.options.container); f.toggleClass("open", !b(this).hasClass("open")); f.append(e.$menu) }); b(window).resize(function () { c(e.$newElement) }); b(window).on("scroll", function () { c(e.$newElement) }); b("html").on("click", function (i) { if (b(i.target).closest(e.$newElement).length < 1) { f.removeClass("open") } }) }, mobile: function () { this.$element.addClass("mobile-device").appendTo(this.$newElement); if (this.options.container) { this.$menu.hide() } }, refresh: function () { this.$lis = null; this.reloadLi(); this.render(); this.setWidth(); this.setStyle(); this.checkDisabled(); this.liHeight() }, update: function () { this.reloadLi(); this.setWidth(); this.setStyle(); this.checkDisabled(); this.liHeight() }, setSelected: function (c, d) { if (this.$lis == null) { this.$lis = this.$menu.find("li") } b(this.$lis[c]).toggleClass("selected", d) }, setDisabled: function (c, d) { if (this.$lis == null) { this.$lis = this.$menu.find("li") } if (d) { b(this.$lis[c]).addClass("disabled").find("a").attr("href", "#").attr("tabindex", -1) } else { b(this.$lis[c]).removeClass("disabled").find("a").removeAttr("href").attr("tabindex", 0) } }, isDisabled: function () { return this.$element.is(":disabled") }, checkDisabled: function () { var c = this; if (this.isDisabled()) { this.$button.addClass("disabled").attr("tabindex", -1) } else { if (this.$button.hasClass("disabled")) { this.$button.removeClass("disabled") } if (this.$button.attr("tabindex") == -1) { if (!this.$element.data("tabindex")) { this.$button.removeAttr("tabindex") } } } this.$button.click(function () { return !c.isDisabled() }) }, tabIndex: function () { if (this.$element.is("[tabindex]")) { this.$element.data("tabindex", this.$element.attr("tabindex")); this.$button.attr("tabindex", this.$element.data("tabindex")) } }, clickListener: function () { var c = this; b("body").on("touchstart.dropdown", ".dropdown-menu", function (d) { d.stopPropagation() }); this.$newElement.on("click", function () { c.setSize(); if (!c.options.liveSearch && !c.multiple) { setTimeout(function () { c.$menu.find(".selected a").focus() }, 10) } }); this.$menu.on("click", "li a", function (n) { var t = b(this).parent().index(), m = c.$element.val(), i = c.$element.prop("selectedIndex"); if (c.multiple) { n.stopPropagation() } n.preventDefault(); if (!c.isDisabled() && !b(this).parent().hasClass("disabled")) { var l = c.$element.find("option"), d = l.eq(t), f = d.prop("selected"), r = d.parent("optgroup"), p = c.options.maxOptions, h = r.data("maxOptions") || false; if (!c.multiple) { l.prop("selected", false); d.prop("selected", true); c.$menu.find(".selected").removeClass("selected"); c.setSelected(t, true) } else { d.prop("selected", !f); c.setSelected(t, !f); if ((p !== false) || (h !== false)) { var o = p < l.filter(":selected").length, j = h < r.find("option:selected").length, s = c.options.maxOptionsText, g = s[0].replace("{n}", p), q = s[1].replace("{n}", h), k = b('<div class="notify"></div>'); if ((p && o) || (h && j)) { if (s[2]) { g = g.replace("{var}", s[2][p > 1 ? 0 : 1]); q = q.replace("{var}", s[2][h > 1 ? 0 : 1]) } d.prop("selected", false); c.$menu.append(k); if (p && o) { k.append(b("<div>" + g + "</div>")); c.$element.trigger("maxReached.bs.select") } if (h && j) { k.append(b("<div>" + q + "</div>")); c.$element.trigger("maxReachedGrp.bs.select") } setTimeout(function () { c.setSelected(t, false) }, 10); k.delay(750).fadeOut(300, function () { b(this).remove() }) } } } if (!c.multiple) { c.$button.focus() } else { if (c.options.liveSearch) { c.$searchbox.focus() } } if ((m != c.$element.val() && c.multiple) || (i != c.$element.prop("selectedIndex") && !c.multiple)) { c.$element.change() } } }); this.$menu.on("click", "li.disabled a, li dt, li .div-contain, .popover-title, .popover-title :not(.close)", function (d) { if (d.target == this) { d.preventDefault(); d.stopPropagation(); if (!c.options.liveSearch) { c.$button.focus() } else { c.$searchbox.focus() } } }); this.$menu.on("click", ".popover-title .close", function () { c.$button.focus() }); this.$searchbox.on("click", function (d) { d.stopPropagation() }); this.$menu.on("click", ".actions-btn", function (d) { if (c.options.liveSearch) { c.$searchbox.focus() } else { c.$button.focus() } d.preventDefault(); d.stopPropagation(); if (b(this).is(".bs-select-all")) { c.selectAll() } else { c.deselectAll() } c.$element.change() }); this.$element.change(function () { c.render(false) }) }, liveSearchListener: function () { var d = this, c = b('<li class="no-results"></li>'); this.$newElement.on("click.dropdown.data-api", function () { d.$menu.find(".active").removeClass("active"); if (!!d.$searchbox.val()) { d.$searchbox.val(""); d.$lis.not(".is-hidden").removeClass("hide"); if (!!c.parent().length) { c.remove() } } if (!d.multiple) { d.$menu.find(".selected").addClass("active") } setTimeout(function () { d.$searchbox.focus() }, 10) }); this.$searchbox.on("input propertychange", function () { if (d.$searchbox.val()) { d.$lis.not(".is-hidden").removeClass("hide").find("a").not(":icontains(" + d.$searchbox.val() + ")").parent().addClass("hide"); if (!d.$menu.find("li").filter(":visible:not(.no-results)").length) { if (!!c.parent().length) { c.remove() } c.html(d.options.noneResultsText + ' "' + d.$searchbox.val() + '"').show(); d.$menu.find("li").last().after(c) } else { if (!!c.parent().length) { c.remove() } } } else { d.$lis.not(".is-hidden").removeClass("hide"); if (!!c.parent().length) { c.remove() } } d.$menu.find("li.active").removeClass("active"); d.$menu.find("li").filter(":visible:not(.divider)").eq(0).addClass("active").find("a").focus(); b(this).focus() }); this.$menu.on("mouseenter", "a", function (f) { d.$menu.find(".active").removeClass("active"); b(f.currentTarget).parent().not(".disabled").addClass("active") }); this.$menu.on("mouseleave", "a", function () { d.$menu.find(".active").removeClass("active") }) }, val: function (c) { if (c !== undefined) { this.$element.val(c); this.$element.change(); return this.$element } else { return this.$element.val() } }, selectAll: function () { if (this.$lis == null) { this.$lis = this.$menu.find("li") } this.$element.find("option:enabled").prop("selected", true); b(this.$lis).filter(":not(.disabled)").addClass("selected"); this.render(false) }, deselectAll: function () { if (this.$lis == null) { this.$lis = this.$menu.find("li") } this.$element.find("option:enabled").prop("selected", false); b(this.$lis).filter(":not(.disabled)").removeClass("selected"); this.render(false) }, keydown: function (p) { var q, o, i, n, k, j, r, f, h, m, d, s, g = { 32: " ", 48: "0", 49: "1", 50: "2", 51: "3", 52: "4", 53: "5", 54: "6", 55: "7", 56: "8", 57: "9", 59: ";", 65: "a", 66: "b", 67: "c", 68: "d", 69: "e", 70: "f", 71: "g", 72: "h", 73: "i", 74: "j", 75: "k", 76: "l", 77: "m", 78: "n", 79: "o", 80: "p", 81: "q", 82: "r", 83: "s", 84: "t", 85: "u", 86: "v", 87: "w", 88: "x", 89: "y", 90: "z", 96: "0", 97: "1", 98: "2", 99: "3", 100: "4", 101: "5", 102: "6", 103: "7", 104: "8", 105: "9" }; q = b(this); i = q.parent(); if (q.is("input")) { i = q.parent().parent() } m = i.data("this"); if (m.options.liveSearch) { i = q.parent().parent() } if (m.options.container) { i = m.$menu } o = b("[role=menu] li:not(.divider) a", i); s = m.$menu.parent().hasClass("open"); if (!s && /([0-9]|[A-z])/.test(String.fromCharCode(p.keyCode))) { if (!m.options.container) { m.setSize(); m.$menu.parent().addClass("open"); s = m.$menu.parent().hasClass("open") } else { m.$newElement.trigger("click") } m.$searchbox.focus() } if (m.options.liveSearch) { if (/(^9$|27)/.test(p.keyCode) && s && m.$menu.find(".active").length === 0) { p.preventDefault(); m.$menu.parent().removeClass("open"); m.$button.focus() } o = b("[role=menu] li:not(.divider):visible", i); if (!q.val() && !/(38|40)/.test(p.keyCode)) { if (o.filter(".active").length === 0) { o = m.$newElement.find("li").filter(":icontains(" + g[p.keyCode] + ")") } } } if (!o.length) { return } if (/(38|40)/.test(p.keyCode)) { n = o.index(o.filter(":focus")); j = o.parent(":not(.disabled):visible").first().index(); r = o.parent(":not(.disabled):visible").last().index(); k = o.eq(n).parent().nextAll(":not(.disabled):visible").eq(0).index(); f = o.eq(n).parent().prevAll(":not(.disabled):visible").eq(0).index(); h = o.eq(k).parent().prevAll(":not(.disabled):visible").eq(0).index(); if (m.options.liveSearch) { o.each(function (e) { if (b(this).is(":not(.disabled)")) { b(this).data("index", e) } }); n = o.index(o.filter(".active")); j = o.filter(":not(.disabled):visible").first().data("index"); r = o.filter(":not(.disabled):visible").last().data("index"); k = o.eq(n).nextAll(":not(.disabled):visible").eq(0).data("index"); f = o.eq(n).prevAll(":not(.disabled):visible").eq(0).data("index"); h = o.eq(k).prevAll(":not(.disabled):visible").eq(0).data("index") } d = q.data("prevIndex"); if (p.keyCode == 38) { if (m.options.liveSearch) { n -= 1 } if (n != h && n > f) { n = f } if (n < j) { n = j } if (n == d) { n = r } } if (p.keyCode == 40) { if (m.options.liveSearch) { n += 1 } if (n == -1) { n = 0 } if (n != h && n < k) { n = k } if (n > r) { n = r } if (n == d) { n = j } } q.data("prevIndex", n); if (!m.options.liveSearch) { o.eq(n).focus() } else { p.preventDefault(); if (!q.is(".dropdown-toggle")) { o.removeClass("active"); o.eq(n).addClass("active").find("a").focus(); q.focus() } } } else { if (!q.is("input")) { var c = [], l, t; o.each(function () { if (b(this).parent().is(":not(.disabled)")) { if (b.trim(b(this).text().toLowerCase()).substring(0, 1) == g[p.keyCode]) { c.push(b(this).parent().index()) } } }); l = b(document).data("keycount"); l++; b(document).data("keycount", l); t = b.trim(b(":focus").text().toLowerCase()).substring(0, 1); if (t != g[p.keyCode]) { l = 1; b(document).data("keycount", l) } else { if (l >= c.length) { b(document).data("keycount", 0); if (l > c.length) { l = 1 } } } o.eq(c[l - 1]).focus() } } if (/(13|32|^9$)/.test(p.keyCode) && s) { if (!/(32)/.test(p.keyCode)) { p.preventDefault() } if (!m.options.liveSearch) { b(":focus").click() } else { if (!/(32)/.test(p.keyCode)) { m.$menu.find(".active a").click(); q.focus() } } b(document).data("keycount", 0) } if ((/(^9$|27)/.test(p.keyCode) && s && (m.multiple || m.options.liveSearch)) || (/(27)/.test(p.keyCode) && !s)) { m.$menu.parent().removeClass("open"); m.$button.focus() } }, hide: function () { this.$newElement.hide() }, show: function () { this.$newElement.show() }, destroy: function () { this.$newElement.remove(); this.$element.remove() } }; b.fn.selectpicker = function (e, f) { var c = arguments; var g; var d = this.each(function () { if (b(this).is("select")) { var m = b(this), l = m.data("selectpicker"), h = typeof e == "object" && e; if (!l) { m.data("selectpicker", (l = new a(this, h, f))) } else { if (h) { for (var j in h) { l.options[j] = h[j] } } } if (typeof e == "string") { var k = e; if (l[k] instanceof Function) { [].shift.apply(c); g = l[k].apply(l, c) } else { g = l.options[k] } } } }); if (g !== undefined) { return g } else { return d } }; b.fn.selectpicker.defaults = { style: "btn-default", size: "auto", title: null, selectedTextFormat: "values", noneSelectedText: "Chọn tất cả", noneResultsText: "No results match", countSelectedText: "{0} of {1} selected", maxOptionsText: ["Limit reached ({n} {var} max)", "Group limit reached ({n} {var} max)", ["items", "item"]], width: false, container: false, hideDisabled: false, showSubtext: false, showIcon: true, showContent: true, dropupAuto: true, header: false, liveSearch: false, actionsBox: false, multipleSeparator: ", ", iconBase: "glyphicon", tickIcon: "glyphicon-ok", maxOptions: false }; b(document).data("keycount", 0).on("keydown", ".bootstrap-select [data-toggle=dropdown], .bootstrap-select [role=menu], .bootstrap-select-searchbox input", a.prototype.keydown).on("focusin.modal", ".bootstrap-select [data-toggle=dropdown], .bootstrap-select [role=menu], .bootstrap-select-searchbox input", function (c) { c.stopPropagation() }) }(window.jQuery);

// Init
minScreen = 767;
if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
    $('.multiselect').multiselect({
        buttonClass: function (options) {
            if (options.length == 0) {
                return 'text-left text-ellipsis placeholder';
            }
            else {
                return 'text-left text-ellipsis';
            }
        },
        buttonWidth: '100%',
        buttonContainer: '<div class="btn-group form-control" />',
        maxHeight: false,
        buttonText: function (options) {
            if (options.length == 0) {
                //console.log($(options.context).attr('placeholder'));
                if ($(options.context).attr('placeholder'))
                    return $(options.context).attr('placeholder') + ' <b class="caret"></b>';
                if (options.context.id.indexOf("DistrictIds") != -1) return 'Tất cả Quận / Huyện <b class="caret"></b>';
                if (options.context.id.indexOf("WardIds") != -1) return 'Tất cả Phường / Xã <b class="caret"></b>';
                if (options.context.id.indexOf("StreetIds") != -1) return 'Tất cả các Đường / Phố <b class="caret"></b>';
                if (options.context.id.indexOf("DirectionIds") != -1) return 'Tất cả các Hướng <b class="caret"></b>';
                if (options.context.id.indexOf("TypeIds") != -1) return 'Tất cả các loại BĐS <b class="caret"></b>';
                if (options.context.id.indexOf("AnyType") != -1) return 'Tất cả các trạng thái <b class="caret"></b>';
                if (options.context.id.indexOf("ApartmentIds") != -1) return 'Tất cả dự án / chung cư <b class="caret"></b>';
                if (options.context.id.indexOf("ServedUserIds") != -1) return '-- NV dẫn -- <b class="caret"></b>';
                return '-- Vui lòng chọn -- <b class="caret"></b>';
            }
            else {
                var selected = '';
                options.each(function () {
                    selected += $(this).text() + ', ';
                });
                return selected.substr(0, selected.length - 2) + ' <b class="caret"></b>';
            }
        },
        includeSelectAllOption: false,
        selectAllText: 'Chọn tất cả',
        selectAllValue: 0,
        enableFiltering: true,
        enableCaseInsensitiveFiltering: true,
        filterPlaceholder: 'Tìm kiếm...',
        onChange: function (element, checked) {
            var select = $(element).parent();
            var multiselect = select.parent().find('.multiselect');
            multiselect.toggleClass('placeholder', select.val() == null);
        }
    });
}

if ($(window).width() < minScreen) {
    $('.selectpicker').selectpicker();
    //$('#WardIds, #StreetIds, #DirectionIds').closest('.form-group').hide();
}

var selectedDistrictIds;
var selectedWardIds;
var selectedStreetIds;
var selectedApartmentIds;
var selectedTopicIds;
if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
    $("select[id$=ProvinceId]").change(function () {
        if ($(this).val() > 0) {
            var slcDistrictIds = $("#" + this.id.replace("ProvinceId", "DistrictIds"), $(this).closest('form'));
            if (slcDistrictIds.length > 0) {
                $.ajax({
                    type: "get",
                    dataType: "JSON",
                    url: "/RealEstate.Admin/Home/GetDistrictsForJson",
                    data: {
                        provinceId: $(this).val()
                    },
                    success: function (response) {
                        var selectedValues = slcDistrictIds.val();
                        slcDistrictIds.empty();
                        $.each(response.list, function (i, item) { slcDistrictIds.append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
                        if (selectedDistrictIds != null) slcDistrictIds.val(selectedDistrictIds);
                        else slcDistrictIds.val(selectedValues);
                        slcDistrictIds.change();
                        if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                            slcDistrictIds.multiselect('rebuild');
                        } else {
                            slcDistrictIds.selectpicker('refresh');
                        }
                    },
                    error: function (request, status, error) {
                        slcDistrictIds.empty().change();
                        if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                            slcDistrictIds.multiselect('rebuild');
                        } else {
                            slcDistrictIds.selectpicker('refresh');
                        }
                    }
                });
            }
        }
        return false;
    });

    $("select[id$=DistrictIds]").change(function () {

        var slcWardIds = $("#" + this.id.replace("DistrictIds", "WardIds"), $(this).closest('form'));
        var slcStreetIds = $("#" + this.id.replace("DistrictIds", "StreetIds"), $(this).closest('form'));
        var slcStreetId = $("#" + this.id.replace("DistrictIds", "StreetId"), $(this).closest('form'));
        var slcApartmentIds = $("#" + this.id.replace("DistrictIds", "ApartmentIds"), $(this).closest('form'));
        var slcApartmentId = $("#" + this.id.replace("DistrictIds", "ApartmentId"), $(this).closest('form'));

        if ($(this).val()) {

            // Get Wards
            if (slcWardIds.length > 0) {
                $.ajax({
                    type: "get",
                    dataType: "JSON",
                    traditional: true,
                    url: "/RealEstate.Admin/Home/GetWardsByDistrictsForJson",
                    data: {
                        districtIds: $(this).val()
                    },
                    success: function (response) {
                        var selectedValues = slcWardIds.val();
                        slcWardIds.empty();
                        $.each(response.list, function (i, item) { slcWardIds.append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
                        if (selectedWardIds != null) slcWardIds.val(selectedWardIds);
                        else slcWardIds.val(selectedValues);
                        slcWardIds.change();
                        if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                            slcWardIds.multiselect('rebuild');
                        } else {
                            slcWardIds.selectpicker('refresh');
                        }
                    },
                    error: function (request, status, error) {
                        slcWardIds.empty().change();
                        if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                            slcWardIds.multiselect('rebuild');
                        } else {
                            slcWardIds.selectpicker('refresh');
                        }
                    }
                });
            }

            // Get Streets
            if (slcStreetIds.length > 0 || slcStreetId.length > 0) {
                $.ajax({
                    type: "get",
                    dataType: "JSON",
                    traditional: true,
                    url: "/RealEstate.Admin/Home/GetStreetsByDistrictsForJson",
                    data: {
                        districtIds: $(this).val()
                    },
                    success: function (response) {
                        if (slcStreetIds.length > 0) {
                            var selectedValues = slcStreetIds.val();
                            slcStreetIds.empty();
                            $.each(response.list, function (i, item) { slcStreetIds.append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
                            if (selectedStreetIds != null) slcStreetIds.val(selectedStreetIds);
                            else slcStreetIds.val(selectedValues);
                            slcStreetIds.change();
                            if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                                slcStreetIds.multiselect('rebuild');
                            } else {
                                slcStreetIds.selectpicker('refresh');
                            }
                        }

                        if (slcStreetId.length > 0) {
                            var selectedValue = slcStreetId.val();
                            slcStreetId.empty().append("<option value=''></option>");
                            $.each(response.list, function (i, item) { slcStreetId.append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
                            slcStreetId.val(selectedValue);
                            slcStreetId.change();
                            if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                                if ($.fn.combobox) slcStreetId.combobox('refresh');
                            }
                        }
                    },
                    error: function (request, status, error) {
                        if (slcStreetIds.length > 0) {
                            slcStreetIds.empty().change();
                            if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                                slcStreetIds.multiselect('rebuild');
                            } else {
                                slcStreetIds.selectpicker('refresh');
                            }
                        }
                        if (slcStreetId.length > 0) {
                            slcStreetId.empty().append("<option value=''></option>").change();
                            if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                                if ($.fn.combobox) slcStreetId.combobox('refresh')
                            }
                        }
                    }
                });
            }

            //Get Apartment
            if (slcApartmentIds.length > 0 || slcApartmentId.length > 0) {
                $.ajax({
                    type: "get",
                    dataType: "JSON",
                    traditional: true,
                    url: "/RealEstate.Admin/Home/GetApartmentsByDistrictsForJson",
                    data: {
                        districtIds: $(this).val()
                    },
                    success: function (response) {

                        if (slcApartmentIds.length > 0) {
                            var selectedValues = slcApartmentIds.val();
                            slcApartmentIds.empty();
                            $.each(response.list, function (i, item) { slcApartmentIds.append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
                            if (selectedApartmentIds != null) slcWardIds.val(selectedApartmentIds);
                            else slcApartmentIds.val(selectedValues);
                            slcApartmentIds.change();
                            if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                                slcApartmentIds.multiselect('rebuild');
                            } else {
                                slcApartmentIds.selectpicker('refresh');
                            }
                        }

                        if (slcApartmentId.length > 0) {
                            var selectedValue = slcApartmentId.val();
                            slcApartmentId.empty().append("<option value=''></option>");
                            $.each(response.list, function (i, item) { slcApartmentId.append("<option value=" + item.Value + ">" + item.Text + "</option>"); });
                            slcApartmentId.val(selectedValue);
                            slcApartmentId.change();
                            if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                                if ($.fn.combobox) slcApartmentId.combobox('refresh');
                            }
                        }
                    },
                    error: function (request, status, error) {
                        if (slcApartmentIds.length > 0) {
                            slcApartmentIds.empty().change();
                            if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                                slcApartmentIds.multiselect('rebuild');
                            } else {
                                slcApartmentIds.selectpicker('refresh');
                            }
                        }
                        if (slcApartmentId.length > 0) {
                            slcApartmentId.empty().append("<option value=''></option>").change();
                            if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                                if ($.fn.combobox) slcApartmentId.combobox('refresh')
                            }
                        }
                    }
                });
            }
        }
        else {
            if (slcWardIds.length > 0) {
                slcWardIds.empty().change();
                if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                    slcWardIds.multiselect('rebuild');
                } else {
                    slcWardIds.selectpicker('refresh');
                }
            }
            if (slcStreetIds.length > 0) {
                slcStreetIds.empty().change();
                if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                    slcStreetIds.multiselect('rebuild');
                } else {
                    slcStreetIds.selectpicker('refresh');
                }
            }
            if (slcStreetId.length > 0) {
                slcStreetId.empty().append("<option value=''></option>").change();
                if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                    if ($.fn.combobox) slcStreetId.combobox('refresh');
                }
            }
            if (slcApartmentIds.length > 0) {
                slcApartmentIds.empty().change();
                if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                    slcApartmentIds.multiselect('rebuild');
                } else {
                    slcApartmentIds.selectpicker('refresh');
                }
            }
            if (slcApartmentId.length > 0) {
                slcApartmentId.empty().append("<option value=''></option>").change();
                if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                    if ($.fn.combobox) slcApartmentId.combobox('refresh')
                }
            }
        }
        return false;
    });
}

// Filter post forum
$("select[id$=ThreadIdIndex]").change(function () {
    var ThreadId = $(this).val();
    if ($(this).val() > 0) {
        var slcTopicIds = $("#" + this.id.replace("ThreadIdIndex", "TopicIds"));
        if (slcTopicIds.length > 0) {
            $.ajax({
                type: "post",
                dataType: "JSON",
                url: "/ajax/Admin/RealEstate.MiniForum/LoadTopic",
                data: {
                    ThreadId: ThreadId,
                    __RequestVerificationToken: antiForgeryToken
                },
                success: function (response) {
                    var selectedValues = slcTopicIds.val();
                    slcTopicIds.empty();
                    $.each(response.list, function (i, item) { slcTopicIds.append("<option value=" + item.Id + ">" + item.Value + "</option>"); });
                    if (selectedTopicIds != null) slcTopicIds.val(selectedTopicIds);
                    else slcTopicIds.val(selectedValues);
                    slcTopicIds.change();
                    if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                        slcTopicIds.multiselect('rebuild');
                    }
                },
                error: function (request, status, error) {
                    slcTopicIds.empty().change();
                    if ($(window).width() > minScreen) {// kiem tra kich thuoc man hinh cua thiet bi
                        slcTopicIds.multiselect('rebuild');
                    }
                }
            });
        }
    }
    return false;
});