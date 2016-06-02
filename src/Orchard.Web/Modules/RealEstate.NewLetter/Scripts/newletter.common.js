$(function () {
	// auto submit form when select change
	$("#publishActions").change(function () {
		var action = $(this).val();
		switch (action) {
			
			default:
				if ($(':checkbox[name$=".IsChecked"]:checked').length > 0) {
					$('#ReturnUrl').val(location);
					$('button[name="submit.BulkEdit"]').click();
				}
				else {
					alert('Vui lòng chọn dữ liệu cần cập nhật.')
					$(this).val('');
				}
				break;
		}
	});

	// Table Hover - Table Select
	$('table.items tbody tr').hover(function () { $(this).addClass('highlight') }, function () { $(this).removeClass('highlight') });
	$('table.items tbody tr td input[type=checkbox]').change(function () { $(this).closest('tr').toggleClass('selected', $(this).is(':checked')) });
	$('.selectAll').change(function () { $('input:checkbox[name^=' + this.value + ']').attr('checked', this.checked).change() });

});