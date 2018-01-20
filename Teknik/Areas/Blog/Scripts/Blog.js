$(document).ready(function () {
    $("textarea.mdd_editor").MarkdownDeep({
        help_location: helpURL,
        disableTabHandling: false,
        resizebar: false,
        SafeMode: true,
        ExtraMode: true,
        MarkdownInHtml: true
    });

    $("#comment_submit").click(function () {
        $('#newComment').modal('hide');
        postID = $("#post_id").val();
        post = $("#comment_post").val();
        $.ajax({
            type: "POST",
            url: addCommentURL,
            data: { postID: postID, article: post },
            success: function (response) {
                if (response.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
        return false;
    });

    $('#editComment').on('show.bs.modal', function (e) {
        $("#edit_comment_post").val("");
        commentID = $(e.relatedTarget).attr("id");
        $("#edit_comment_postid").val(commentID);
        $.ajax({
            type: "POST",
            url: getCommentArticleURL,
            data: { commentID: commentID },
            success: function (response) {
                if (response.result) {
                    $("#edit_comment_post").val(response.result);
                }
            }
        });
    });

    $("#edit_comment_submit").click(function () {
        $('#editComment').modal('hide');
        postID = $("#edit_comment_postid").val();
        post = $("#edit_comment_post").val();
        $.ajax({
            type: "POST",
            url: editCommentURL,
            data: { commentID: postID, article: post },
            success: function (response) {
                if (response.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
        return false;
    });
});

function loadMorePosts(start, count) {
    blog_id = $(".blog-main").attr("id");
    $.ajax({
        type: "POST",
        url: getPostsURL,
        data: { blogID: blog_id, count: count, startPostID: start },
        success: function (response) {
            if (response) {
                $(".blog-main").append(response);
                linkPostDelete('.delete_post');
                linkPostPublish('.publish_post');
                linkPostUnpublish('.unpublish_post');
                $(window).bind('scroll', bindScrollPosts);
            }
        }
    });
}

function loadMoreComments(start, count) {
    post_id = $(".post-comments").attr("id");
    $.ajax({
        type: "POST",
        url: getCommentsURL,
        data: { postID: post_id, count: count, startCommentID: start },
        success: function (response) {
            if (response) {
                $(".post-comments").append(response);
                linkCommentDelete('.delete_comment');
                $(window).bind('scroll', bindScrollComments);
            }
        }
    });
}

function bindScrollPosts() {
    if ($(window).scrollTop() + $(window).height() > $(document).height() - 100) {
        $(window).unbind('scroll');
        loadMorePosts(start_post, posts);
        start_post = start_post + posts;
    }
}

function bindScrollComments() {
    if ($(window).scrollTop() + $(window).height() > $(document).height() - 100) {
        $(window).unbind('scroll');
        loadMoreComments(start_post, posts);
        start_post = start_post + posts;
    }
}

function linkPostUnpublish(selector) {
    $(selector).click(function () {
        var object = $(this);
        post_id = object.attr("id");
        $.ajax({
            type: "POST",
            url: publishPostURL,
            data: { postID: post_id, publish: false },
            success: function (response) {
                if (response.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
    });
}

function linkPostPublish(selector) {
    $(selector).click(function () {
        var object = $(this);
        post_id = object.attr("id");
        $.ajax({
            type: "POST",
            url: publishPostURL,
            data: {postID: post_id, publish: true },
            success: function (response) {
                if (response.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
    });
}

function linkPostDelete(selector) {
    $(selector).click(function () {
        var object = $(this);
        post_id = object.attr("id");
        bootbox.confirm("Are you sure you want to delete your post?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deletePostURL,
                    data: { postID: post_id },
                    success: function (response) {
                        if (response.result) {
                            window.location.reload();
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                        }
                    }
                });
            }
        });
    });
}

function linkCommentDelete(selector) {
    $(selector).click(function () {
        var object = $(this);
        post_id = object.attr("id");
        bootbox.confirm("Are you sure you want to delete your comment?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deleteCommentURL,
                    data: { commentID: post_id },
                    success: function (response) {
                        if (response.result) {
                            window.location.reload();
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                        }
                    }
                });
            }
        });
    });
}
