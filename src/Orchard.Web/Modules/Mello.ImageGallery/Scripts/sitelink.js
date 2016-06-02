var itemsPerPage = 2, ci = itemsPerPage - 1, promotion_total = 0, promotion_minLeft = 0, promotion_maxLeft = 0, promotion_left = 0, promotion_speed = 3000;
function slideInit() {
    objPromo = jQuery('#sitelinkmain');
    //alert(obj);
    items = jQuery('#sitelinkmain > .item');
    promotion_total = s = items.length;
    w = jQuery('.sitelinkmain .imagegallery').width();
    if (promotion_total > itemsPerPage) {
        // Reset width for slider
        objPromo.css('width', w * s + 'px');

        // Set margin left min when click next button
        promotion_minLeft = -((w * itemsPerPage  + 20 ) * (promotion_total % itemsPerPage == 0 ? parseInt(promotion_total / itemsPerPage) : parseInt(promotion_total / itemsPerPage) + 1));
        promotion_maxLeft = -(w * itemsPerPage) - 14;
        // Set slide continuos
        jQuery(objPromo).append(jQuery('#sitelinkmain div.item:lt(' + (itemsPerPage - s % itemsPerPage) + ')').clone());
        for (var i = 0; i < itemsPerPage; i++)
            jQuery(objPromo).prepend(jQuery('#sitelinkmain div.item').eq(s - 1).clone());
        s = jQuery('.item').length;
        jQuery(objPromo).css('width', w * s + 'px').css('margin-left', (-(w * itemsPerPage) - 14) + 'px');

        // Reset left
        promotion_left = -itemsPerPage * w;

        // Mouse click
        jQuery('.sitelink #slidenext').click(function () { slideNext(); });
        jQuery('.sitelink #slideprev').click(function () { slidePrev(); });

        // Auto sliding
        st = setTimeout('slidePrev()', promotion_speed);
    }
    else {
        jQuery('.sitelink #slidenext').attr('disabled', 'disabled');
        jQuery('.sitelink #slideprev').attr('disabled', 'disabled');
    }
}

function slideNext() {
    //console.log("promotion_left: " + promotion_left + " - promotion_minLeft: " + promotion_minLeft);
    promotion_left -= Number((itemsPerPage * w) - 14);
    if (promotion_left < promotion_minLeft) promotion_left = promotion_minLeft;
    //console.log("promotion_left: " + promotion_left);

    jQuery(objPromo).animate(
		{ marginLeft: promotion_left },
		{ queue: false, duration: 800, complete: adjustNext }
	)
    //alert('animate Next');	
    // Auto sliding image
    if (st != null) clearTimeout(st);
    st = setTimeout('slideNext()', promotion_speed);
}

function slidePrev() {
    promotion_left += itemsPerPage * w - 14;
    if (promotion_left > promotion_maxLeft) promotion_left = 0;
    jQuery(objPromo).animate(
		{ marginLeft: promotion_left },
		{ queue: false, duration: 800, complete: adjustPrev }
	)
    //alert('animate Prev');
    // Auto sliding image
    if (st != null) clearTimeout(st);
    st = setTimeout('slidePrev()', promotion_speed);
}

function adjustNext() {
    ci += itemsPerPage;
    if (promotion_left == promotion_minLeft) {
        jQuery('#sitelinkmain div.item:lt(' + itemsPerPage + ')').remove();
        var curInd = ci % promotion_total;
        for (i = 0; i < itemsPerPage; i++) {
            curInd += 1;
            if (curInd > promotion_total - 1) curInd = 0;
            objPromo.append(items.eq(curInd).clone());
        }
        objPromo.css('margin-left', promotion_left + (w * itemsPerPage) + 'px');
        if (ci > promotion_total) ci = ci % promotion_total;
    }
}

function adjustPrev() {
    ci -= itemsPerPage;
    if (promotion_left == 0) {
        jQuery('#sitelinkmain div.item:gt(' + (s - itemsPerPage - 1) + ')').remove();
        var curInd = ci - itemsPerPage;
        if (curInd < 0) curInd = promotion_total + curInd;
        for (var i = 0; i < itemsPerPage; i++) {
            if (curInd < 0) curInd = promotion_total - 1;
            objPromo.prepend(items.eq(curInd).clone());
            curInd--;
        }
        objPromo.css('margin-left', promotion_maxLeft + 'px');
        if (ci < 0) ci = ci += promotion_total;
    }
}