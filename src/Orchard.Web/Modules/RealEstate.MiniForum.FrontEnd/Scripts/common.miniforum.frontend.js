$(function () {
	//Load Topic PageCreate/Edit Post
	$('select[id$=ThreadId]').change(function () {
		var ThreadId = $(this).val();
		if (ThreadId > 0) {
			var slcTopic = $("#" + this.id.replace("ThreadId", "TopicId"));
			if (slcTopic.length > 0) {
				slcTopic.children("option:first").text("[Loading..]");
				$.ajax({
					type: "post",
					url: "/ajax/miniforum/AjaxLoadTopicFrontEnd",
					data: {
						ThreadId: ThreadId,
						__RequestVerificationToken: antiForgeryToken
					},
					success: function (results) {
						slcTopic.empty().append("<option value=''>-- Vui lòng chọn chuyên đề --</option>");
						$.each(results.list, function (i, item) { slcTopic.append("<option value=" + item.Id + ">" + item.Value + "</option>"); });
						slcTopic.change();
					},
					error: function (request, status, error) {
						alert("Có lỗi xảy ra!");
					}
				});
			}
		} else {
			$(slcTopic).empty().append("<option value=''>-- Vui lòng chọn chuyên đề --</option>");
		}
	});

    // Comment Forum
	$("#btnCommentFeed").click(function () {
	    var $that = $(this);
	    var PostId = $('#pageId').val();
	    var ParentCommentId = $('#ParentCommentId').val();
	    if (tinyMCE.get('areaContent').getContent() == "") {
	        alert("Vui lòng nhập nội dung!!");
	        $("#areaContent").focus();
	        return false;
	    }
	    //$($that).attr('disabled', 'disabled');
	    var content = tinyMCE.activeEditor.getContent({ format: 'raw' });
	    AjaxPostComment(PostId, content, ParentCommentId);
	    AjaxCommentToFacebook(content, PostId);

	});

    // ajax loading paging
	$('.ajax-paging-forum').on('click', '.pager a', function () {
	    var container = $(this).closest('.ajax-paging-forum');
	    var $that = $('.ajax-paging-forum');
	    if ($that.length > 0) $('html, body').animate({ scrollTop: $that.offset().top }, 500);
	    container.load($(this).attr('href'), function () {  });
	    return false;
	})
    // PersonalPage
	$('#btnPostHomePage').click(function () {
	    var UserId = $('#userId').val();
	    var content = tinyMCE.get('areaContent').getContent();
	    if (content == "") {
	        alert("Vui lòng nhập nội dung!!");
	        $("#areaContent").focus();
	        return false;
	    }
	    tinyMCE.get('areaContent').setContent('');
	    AjaxPersonalPagePost(content, UserId);
	});

	$('.ajax-postuser-content').each(function (e) {
	    var $that = $(this);
	    var UserId = $('#userId').val();
	    if (UserId != undefined && UserId != 0 && $that.find('.overlay-content-property').length > 0)
	    {
	        $.get('/ajax/forum/AjaxLoadForumPostOfUser', { UserId: UserId }, function (data) {
	            $that.html(data);
	        });
	    }
	})
	$('.ajax-menuuser-content').each(function (e) {
	    var $that = $(this);
	    var UserId = $('#userId').val();
	    if (UserId != undefined && UserId != 0 && $that.find('.overlay-content-property').length > 0) {
	        $.get('/ajax/forum/AjaxLoadForumMenuOfUser', { UserId: UserId }, function (data) {
	            $that.html(data);
	        });
	    }
	})
	$('.ajax-content-inbox').each(function (e) {
	    var $that = $(this);
	    if ($that.find('.overlay-content-property').length > 0) {
	        $.get($that.data('url'), function (data) {
	            $that.html(data);
	        });
	    }
	})
	$('.ajax-event-content').click(function (e) {
	    var $that = $(this);
	    var $idcontent = $that.children('a').attr('href');
	    if ($that.data('url') != undefined && $($idcontent).find('.overlay-content-property').length > 0) {
	        $.get($that.data('url'), function (data) {
	            $($idcontent).html(data);
	        });
	    }
	})

	$('#btnSearchFriend').click(function () {
	    var loading = '<div class="overlay-content-property">\
                            <img src="/Themes/Bootstrap/Styles/images/bigrotation2.gif" class="img-load" alt="loading..." />\
                            <p>Đang tải...</p>\
                        </div>';
	    $('.ajax-content').html(loading);
	    var $that = $(this);
	    $.get('/ajax/blog-ca-nhan/AjaxSearchFriend', { kw: $($that).prev().val() }, function (data) {
	        $('.ajax-content').html(data);
	    });
	});

	$('.append-tags').append($('.tags'))
});

function ChangeStatusPost(PostId)
{
    if (confirm('Bạn muốn thay đổi trạng thái đăng bài?')) {
        $.ajax({
            type: "post",
            url: "/ajax/forum/changestatus-post",
            data: {
                PostId: PostId,
                __RequestVerificationToken: antiForgeryToken
            },
            success: function (results) {
                if (results.status) {
                    if (results.IsAction) {
                        alert('Bài viết đã được khôi phục.');
                    } else {
                        alert('Bài viết đã được đưa vào thùng rác.');
                    }
                }
                else {
                    alert(results.message);
                }
            },
            error: function (request, status, error) {
                //alert("Có lỗi xảy ra!");
            }
        });
    }
}
function AjaxLoadCommentByPostId(PostId)// Load comment
{
    //var loadding = "<div class='overlay-content-property'><img src='/Themes/Bootstrap/Styles/images/bigrotation2.gif' id='img-load' alt='Loading...' /><p>Loading...</p></div>";
    $.get('/ajax/miniforum/AjaxLoadComment?PostId=' + PostId, function (data) {
        $("#lstIdComment").html(data);
    });
}
function ChildComment($this)
{
    var PostId = $('#pageId').val();
    var ParentCommentId = $('#ParentCommentId').val();
    var content = $($this).prev().prev().val();
    if (content == '' || content == null) {
        alert('Vui lòng nhập nội dung bình luận của bạn.');
        return false;
    }
    var loading = "<div class='overlay-content-property'><img src='/Themes/Bootstrap/Styles/images/bigrotation2.gif' id='img-load' alt='Loading...' /><p>Loading...</p></div>";
    $("#lstIdComment").html(loading);
    AjaxPostComment(PostId, content, ParentCommentId);
}

function AjaxPostComment(PostId, Content, ParentCommentId)// Post comment
{
    $.ajax({
        type: "post",
        url: "/ajax/miniforum/AjaxSubmitCommentForum",
        data: {
            PostId: PostId, Content: Content, ParentCommentId : ParentCommentId,
            __RequestVerificationToken: antiForgeryToken
        },
        success: function (results) {
            if (results.status) {
                AjaxLoadCommentByPostId(PostId);
                $('#ParentCommentId').val(0);
                tinyMCE.get('areaContent').setContent('');
            } else {
                alert(results.message);
            }
        },
        error: function (request, status, error) {
           // alert("Có lỗi xảy ra!");
        }
    });
}
function AjaxCommentToFacebook(Content,PostId)// Post comment to your wall facebook
{
    $.ajax({
        type: "post",
        url: "/ajax/miniforum/AjaxPostToFacebook",
        data: {
            Content: Content, PostId: PostId,
            __RequestVerificationToken: antiForgeryToken
        },
        success: function (results) {
            console.log('fs');
        },
        error: function (request, status, error) {
            console.log('Error');
        }
    });
}
function ShowReply(IdComment)
{
    $('#form-reply-' + IdComment).slideToggle('slow', function () {
        if ($(this).is(':visible')) { $('#ParentCommentId').val(IdComment); $('#arrow-' + IdComment).show(); }
        else {
            $('#ParentCommentId').val(0);
            $('#arrow-' + IdComment).hide();
        }
    });
}
function DeleteComment(CommentId)
{
    if (confirm('Bạn muốn xóa comment này?')) {
        var PostId = $('#pageId').val();
        $.ajax({
            type: "post",
            url: "/ajax/miniforum/AjaxDeleteComment",
            data: {
                CommentId: CommentId, PostId : PostId,
                __RequestVerificationToken: antiForgeryToken
            },
            success: function (results) {
                //alert('Xóa thành công.');
                AjaxLoadCommentByPostId(PostId);
            },
            error: function (request, status, error) {
                console.log('Error');
            }
        });
    }
}
//Trang cá nhân
function AjaxPersonalPagePost(Content, UserId) {
    $.ajax({
        type: "post",
        url: "/ajax/miniforum/AjaxPersonalPostPage",
        data: {
            Content: Content, UserId : UserId,
            __RequestVerificationToken: antiForgeryToken
        },
        success: function (results) {
            if (results.status) {
                var deleteContent = '';
                if (results.IsAdminOrManagement) {
                    deleteContent = '<i class="icon_rev_cross pointer pull-right" onclick="PerDeletePost(' + results.PostId + ')" title="Xóa"></i>';
                }
                var insertContent = '<article class="post-item media well well-sm">'+
                '<div class="row-blog-header">'+
                    '<div class="blog-left">'+
                        '<img src="' + results.Avatar + '?width=50" class="img-thumbnail" alt="' + results.DisplayName + '" />' +
                    '</div>'+
                    '<div class="blog-right">'+
                        '<div class="row-blog-username">'+
                            '<a href="/blog-ca-nhan" class="text-bold" title="Trang cá nhân">' + results.DisplayName + '</a>' +
                            deleteContent +
                        '</div>'+
                        '<div class="row-blog-timeago">' + results.timeago + '</div>' +
                    '</div>'+
                    '<div class="clearfix"></div>'+
                '</div>'+
                    '<div class="row-blog-main-content media-body">' + Content + '</div>' +
                    '<div class="c-btn-reply" style="margin-bottom: 5px;">'+
                '<span><a href="javascript:PersonalPageShowReply(' + results.PostId + ',0)">Bình luận </a></span>' +
                '<span><a href="javascript:;">Thích </a></span>' +
            '</div>' +
            '<div class="row-form-comment" id="form-reply-' + results.PostId + '">' +
                '<i class="icons-arrow-reply personalpage-post" id="arrow-' + results.PostId + '" style="display:none;"></i>' +
                '<div class="form-background-comment">' +
                    '<textarea class="form-control" cols="12" id="replyContent_' + results.PostId + '" placeholder="Nội dung trả lời"></textarea>' +
                    '<div class="height-10"></div>' +
                    '<a class="btn btn-default btnReplyComent pull-right" onclick="PersonalPostComment(' + results.PostId + ',0)">Đăng</a>' +
                    '<div class="clearfix"></div>' +
                '</div>' +
            '</div>';
                $('#listPostContent').prepend(insertContent);
            } else {
                alert(results.message);
            }
        },
        error: function (request, status, error) {
            //alert("Có lỗi xảy ra!");
        }
    });
}
function PersonalPageShowReply(PostId, ParentCommentId)
{
    $('#form-reply-' + PostId).slideToggle('slow', function () {
        if ($(this).is(':visible')) { $('#ParentCommentId').val(ParentCommentId); $('#arrow-' + PostId).show(); }
        else {
            $('#ParentCommentId').val(0);
            $('#arrow-' + PostId).hide();
        }
    });
}
function PersonalPostComment(PostId,ParentCommentId)
{
    var content = $('#replyContent_' + PostId).val();
    if (content == '')
    {
        alert('Vui lòng nhập nội dung bình luận.');
        $('#replyContent_' + PostId).focus();
        return false;
    }
    $.ajax({
        type: "post",
        url: "/ajax/miniforum/AjaxSubmitCommentForum",
        data: {
            PostId: PostId, Content: content, ParentCommentId: ParentCommentId,
            __RequestVerificationToken: antiForgeryToken
        },
        success: function (results) {
            if (results.status) {
                var deleteContent = '';
                if (results.IsAdminOrManagement) {
                    deleteContent = '<i class="icon_rev_cross pointer pull-right" onclick="PerDeleteComment(' + results.Id + ')" title="Xóa"></i>';
                }
                var insertContent = '<div class="row-comment-item" id="comment-' + results.Id + '">' +
                        '<div class="c-comment-left c-content">' +
                            '<img src="' + results.AvatarUser + '?width=68" class="img-thumbnail" />' +
                        '</div>' +
                        '<div class="c-comment-right">' +
                            '<div class="">' +
                                '<a href="/blog-ca-nhan"><strong>' + results.DisplayName + '</strong></a>' +
                                deleteContent +
                            '</div>' +
                            '<div class="media-body">' +
                                '<div>' + content + '</div>' +
                                '<div class="per-time">' + results.TimeAgo + '</div>' +
                                '<div class="c-btn-reply">' +
                                    '<span><a href="javascript:PerShowSubReply(' + results.Id + ')">Bình luận </a></span>' +
                                    '<span><a href="javascript:;">Thích </a></span>' +
                                '</div>' +
                            '</div>' +
                        '</div>' +
                        '<div class="clearfix"></div>' +
                        '</div>'+
                    '<div class="row-form-subcomment" id="row-form-reply-' + results.Id + '" style="display:none;">'+
                    '<i class="icons-arrow-reply detail-post"></i>'+
                        '<div class="form-background reply-bottom">' +
                            '<textarea class="form-control" cols="12" id="replyContent_'+results.Id+'" placeholder="Nội dung trả lời"></textarea>'+
                            '<div class="height-10"></div>'+
                            '<a class="btn btn-default btnReplyComent pull-right" onclick="PerChildComment(this,' + PostId + ',' + results.Id + ')">Đăng</a>' +
                            '<div class="clearfix"></div>' +
                        '</div>'+
                    '</div>';
                $(insertContent).insertAfter($('#form-reply-' + PostId));
                $('#replyContent_' + PostId).val('');
                PersonalPageShowReply(PostId, ParentCommentId);
            } 
            else 
            {
                alert(results.message);
            }
        },
        error: function (request, status, error) {
            alert("Có lỗi xảy ra!");
        }
    });
}
function PerShowSubReply(CommentId) {
    $('#row-form-reply-' + CommentId).slideToggle('slow', function () {
        if ($(this).is(':visible')) { $('#ParentCommentId').val(CommentId); }
        else {
            $('#ParentCommentId').val(0);
        }
    });
}
function PerChildComment($this, PostId, CommentId) {
    var content = $('#replyContent_' + CommentId).val();
    if (content == '') {
        alert('Vui lòng nhập nội dung bình luận.');
        $('#replyContent_' + CommentId).focus();
        return false;
    }
    $.ajax({
        type: "post",
        url: "/ajax/miniforum/AjaxSubmitCommentForum",
        data: {
            PostId: PostId, Content: content, ParentCommentId: CommentId,
            __RequestVerificationToken: antiForgeryToken
        },
        success: function (results) {
            if (results.status) {
                var deleteContent = '';
                if (results.IsAdminOrManagement) {
                    deleteContent = '<i class="icon_rev_cross pointer pull-right" onclick="PerDeleteComment(' + results .Id+ ')" title="Xóa"></i>';
                }
                var insertContent = '<div class="row-content-subcomment">'+
                    '<i class="icons-arrow-reply detail-post"></i>'+
                            '<div class="form-background reply-bottom" id="comment-' + results.Id + '">' +
                                '<div class="s-comment-left c-content">'+
                                    '<img src="'+results.AvatarUser+'?width=50" class="img-thumbnail" />'+
                                '</div>'+
                                '<div class="s-comment-right">'+
                                    '<div>'+
                                        '<a href="/blog-ca-nhan"><strong>' + results.DisplayName + '</strong></a>' +
                                         deleteContent +
                                    '</div>'+
                                    '<div class="media-body">'+
                                        '<div>' + content + '</div>' +
                                        '<div class="c-btn-reply">'+
                                            '<span class="time">' + results.TimeAgo + '</span>' +
                                        '</div>'+
                                    '</div>'+
                                '</div>'+
                                '<div class="clearfix"></div>'+
                            '</div>'+
                    '</div>';
                $(insertContent).insertAfter($('#row-form-reply-' + CommentId));
                $('#replyContent_' + CommentId).val('');
                PerShowSubReply(CommentId);
            }
            else
            {
                alert(results.message);
            }
        },
        error: function (request, status, error) {
           // alert("Có lỗi xảy ra!");
        }
    });
}
function PerDeletePost(PostId)
{
    if (confirm('Bạn có chắc xóa không?')) {
        $.ajax({
            type: "post",
            url: "/ajax/miniforum/AjaxPerDeletePost",
            data: {
                PostId: PostId,
                __RequestVerificationToken: antiForgeryToken
            },
            success: function (results) {
                if (results.status) {
                    $('#post-' + PostId).remove();
                } else {
                    alert(results.message);
                }
            },
            error: function (request, status, error) {
                alert("Có lỗi xảy ra!");
            }
        });
    }
}
function PerDeleteComment(CommentId)
{
    if (confirm('Bạn có chắc xóa không?')) {
        $.ajax({
            type: "post",
            url: "/ajax/miniforum/AjaxPerDeleteComment",
            data: {
                CommentId: CommentId,
                __RequestVerificationToken: antiForgeryToken
            },
            success: function (results) {
                if (results.status) {
                    $('#comment-' + CommentId).remove();
                    $('#arrow-subcomment-' + CommentId).remove();
                } else {
                    alert(results.message);
                }
            },
            error: function (request, status, error) {
                alert("Có lỗi xảy ra!");
            }
        });
    }
}
//Widget Post Of User
function WidgetDeletePost(PostId) {
    if (confirm('Bạn muốn xóa bài viết này?')) {
        $.ajax({
            type: "post",
            url: "/ajax/forum/AjaxDeletePost",
            data: {
                PostId: PostId,
                __RequestVerificationToken: antiForgeryToken
            },
            success: function (results) {
                if (results.status) {
                    $.get('/ajax/forum/AjaxLoadForumPostOfUser', { UserId: $('#userId').val() }, function (data) {
                        $('.ajax-postuser-content').html(data);
                    });
                }
                else {
                    alert(results.message);
                }
            },
            error: function (request, status, error) {
                //alert("Có lỗi xảy ra!");
            }
        });
    }
}
function WidgetChangeStatus(PostId) {
        if (confirm('Bạn muốn thay đổi trang thái hiển thị bài viết này?')) {
            $.ajax({
                type: "post",
                url: "/ajax/forum/changestatus-post",
                data: {
                    PostId: PostId,
                    __RequestVerificationToken: antiForgeryToken
                },
                success: function (results) {
                    if (results.status) {
                        if (results.IsAction) {
                            alert('Bài viết đã được khôi phục.');
                        } else {
                            alert('Bài viết đã được đưa vào thùng rác.');
                        }
                        $.get('/ajax/forum/AjaxLoadForumPostOfUser', { UserId: $('#userId').val() }, function (data) {
                            $('.ajax-postuser-content').html(data);
                        });
                    }
                    else {
                        alert(results.message);
                    }
                },
                error: function (request, status, error) {
                   // alert("Có lỗi xảy ra!");
                }
            });
        }
}
function GetAvatarUserSelect(userId)
{
    $.ajax({
        type: "post",
        url: "/ajax/miniforum/AjaxGetPhotoUserSelect",
        data: {
            userId: userId,
            __RequestVerificationToken: antiForgeryToken
        },
        success: function (result) {
            var _onlineHtml = '';
            if (result.CheckIsUserOnline) {
                _onlineHtml = '<span title="Đang online" class="icon_status_online"></span>';
            } else {
                _onlineHtml = '<span title="Đã offline" class="icon_status_offline"></span>';
            }
            $("#userIdFriend").text(result.uId);

            var img;
            if (result.Image == '' || result.Image == null) {
                img = '<div class="img-home-no-avatar"></div>';
            }
            else {
                img = '<img src=' + result.Image + ' class="img-responsive img-thumbnail" />';
            }
            $("#userphoto-select").append('<a href="' + result.LinkHomeUserOrther + '">' + img + '</a>');

            var span = '<a href="' + result.LinkHomeUserOrther + '">' + _onlineHtml + '<strong>' + ((result.DisplayName != null || result.DisplayName == "") ? result.DisplayName : result.Username) + '</strong></a>';
            var info = '<a href="' + result.ViewProfileUserOrther + '"><strong>Xem thông tin</strong></a>';
            $("#username-select").append(span);
            $("#view-info-select").append(info);

            $("#nameMessage").html(((result.DisplayName != null || result.DisplayName == "") ? result.DisplayName : result.Username));
            $('#menufriendsendmessage .ClassSendMessage').attr('data-displayname', ((result.DisplayName != null || result.DisplayName == "") ? result.DisplayName : result.Username));
        },
        error: function (request, status, error) {
            console.log('Có lỗi xảy ra');
        }
    });
}
//Tin nhắn
function AjaxDeleteMessageUser(Id,$that) {
    if (confirm('Bạn có chắc xóa không?')) {
        $.ajax({
            type: "post",
            url: "/ajax/control-panel/AjaxDeleteMessageUser",
            data: {
                Id: Id,
                __RequestVerificationToken: antiForgeryToken
            },
            success: function (results) {
                if (results.status) {
                    $($that).closest('.message-row').remove();
                } else {
                    alert(results.message);
                }
            },
            error: function (request, status, error) {
                alert("Có lỗi xảy ra!");
            }
        });
    }
}

//Friend
function CheckFriend(UserSelect)
{
    if (UserSelect != null) {
        $.get('/ajax/blog-ca-nhan/AjaxCheckFriend', { UserSelect: UserSelect }, function (data) {
            $('#btn-f-friend').html(data);
        });
    }
}

function AjaxAddFriend($that) {
    var UserId = $($that).data('userselect');
    $.ajax({
        type: "post",
        url: "/ajax/blog-ca-nhan/AjaxAddFriend",
        data: {
            UserId: UserId,
            __RequestVerificationToken: antiForgeryToken
        },
        success: function (results) {
            if (results.status) {
                $($that).replaceWith('<a class="btn btn-default" disabled="disabled"><i class="glyphicon glyphicon-user text-green"></i> Đã gửi yêu cầu</a>');
            } else {
                alert(results.message);
            }
        },
        error: function (request, status, error) {
            alert("Có lỗi xảy ra!");
        }
    });
}
function AjaxAcceptFriend($that)
{
    var UserId = $($that).data('userselect');
    $.ajax({
        type: "post",
        url: "/ajax/blog-ca-nhan/AjaxAcceptFriend",
        data: {
            UserId: UserId,
            __RequestVerificationToken: antiForgeryToken
        },
        success: function (results) {
            if (results.status) {
                $($that).replaceWith('<button disabled="disabled" class="btn btn-default"><i class="glyphicon glyphicon-user text-green"></i> Bạn bè</button>');
            } else {
                alert(results.message);
            }
        },
        error: function (request, status, error) {
            alert("Có lỗi xảy ra!");
        }
    });
}

function PartialSendMessage(UserId, UserName)//partial-username
{
    $('#messagePartialModal').find('#UserIdMessage').val(UserId);
    $('#messagePartialModal').find('#partial-username').val(UserName);
    $('#messagePartialModal').modal('show');
}
