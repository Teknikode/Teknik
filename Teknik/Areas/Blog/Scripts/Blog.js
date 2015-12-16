$(document).ready(function () {
    $("#blog_submit").click(function () {
        $('#newPost').modal('hide');
        userID = encodeURIComponent($("#blog_userid").val());
        title = encodeURIComponent($("#blog_title").val());
        post = encodeURIComponent($("#blog_post").val());
        $.ajax({
            type: "POST",
            url: "../../../add_post.php",
            data: "userID=" + userID + "&title=" + title + "&post=" + post,
            success: function (html) {
                if (html == 'true') {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html + '</div>');
                }
            }
        });
        return false;
    });

    $('#editPost').on('show.bs.modal', function (e) {
        $("#edit_blog_post").val("");
        userID = encodeURIComponent($(e.relatedTarget).attr("id"));
        $("#edit_blog_postid").val(userID);
        $.ajax({
            type: "POST",
            url: "../../../get_title_content.php",
            data: "id=" + userID,
            success: function (html) {
                if (html) {
                    $("#edit_blog_title").val(html);
                }
            }
        });
        $.ajax({
            type: "POST",
            url: "../../../get_post_content.php",
            data: "id=" + userID,
            success: function (html) {
                if (html) {
                    $("#edit_blog_post").val(html);
                }
            }
        });
    });

    $("#edit_submit").click(function () {
        $('#editPost').modal('hide');
        postID = encodeURIComponent($("#edit_blog_postid").val());
        userID = encodeURIComponent($("#edit_blog_userid").val());
        title = encodeURIComponent($("#edit_blog_title").val());
        post = encodeURIComponent($("#edit_blog_post").val());
        $.ajax({
            type: "POST",
            url: "../../../edit_post.php",
            data: "userID=" + userID + "&postID=" + postID + "&title=" + title + "&post=" + post,
            success: function (html) {
                if (html == 'true') {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html + '</div>');
                }
            }
        });
        return false;
    });

    $("#comment_submit").click(function () {
        $('#newComment').modal('hide');
        postID = encodeURIComponent($("#post_id").val());
        post = encodeURIComponent($("#comment_post").val());
        $.ajax({
            type: "POST",
            url: "../../../includes/add_comment.php",
            data: "postID=" + postID + "&service=blog&comment=" + post,
            success: function (html) {
                if (html == 'true') {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html + '</div>');
                }
            }
        });
        return false;
    });

    $('#editComment').on('show.bs.modal', function (e) {
        $("#edit_comment_post").val("");
        commentID = encodeURIComponent($(e.relatedTarget).attr("id"));
        $("#edit_comment_postid").val(commentID);
        $.ajax({
            type: "POST",
            url: "../../../includes/get_comment_content.php",
            data: "id=" + commentID,
            success: function (html) {
                if (html) {
                    $("#edit_comment_post").val(html);
                }
            }
        });
    });

    $("#edit_comment_submit").click(function () {
        $('#editComment').modal('hide');
        postID = encodeURIComponent($("#edit_comment_postid").val());
        post = encodeURIComponent($("#edit_comment_post").val());
        $.ajax({
            type: "POST",
            url: "../../../includes/edit_comment.php",
            data: "commentID=" + postID + "&post=" + post,
            success: function (html) {
                if (html == 'true') {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html + '</div>');
                }
            }
        });
        return false;
    });

    var imageUpload = $('#upload_image').upload({
        name: 'file',
        action: '../../../includes/upload.php',
        enctype: 'multipart/form-data',
        params: {},
        autoSubmit: true,
        onSubmit: function () {
            $("#image_url").val('Uploading Image...');
        },
        onComplete: function (filename) {
            obj = JSON.parse(filename);
            if (!obj.error) {
                $("#image_url").val(obj.results.file.name);
            }
            else {
                $("#image_url").val('Error Uploading');
            }
        }
    });
});

function loadMorePosts(start, count) {
    blog_id = encodeURIComponent($(".blog-main").attr("id"));
    $.ajax({
        type: "POST",
        url: "../../../get_post.php",
        data: "userID=" + blog_id + "&postCount=" + count + "&startPost=" + start,
        success: function (html) {
            if (html) {
                $(".blog-main").append(html);
                linkPostDelete('.delete_post');
                linkPostPublish('.publish_post');
                linkPostUnpublish('.unpublish_post');
                $(window).bind('scroll', bindScrollPosts);
            }
        }
    });
}

function loadMoreComments(start, count) {
    post_id = encodeURIComponent($(".post-comments").attr("id"));
    $.ajax({
        type: "POST",
        url: "../../../includes/get_comment.php",
        data: "postID=" + post_id + "&service=blog&postCount=" + count + "&startPost=" + start,
        success: function (html) {
            if (html) {
                $(".post-comments").append(html);
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
        post_id = encodeURIComponent(object.attr("id"));
        $.ajax({
            type: "POST",
            url: "../../../publish_post.php",
            data: "publish=0&id=" + post_id,
            success: function (html) {
                if (html == 'true') {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html + '</div>');
                }
            }
        });
    });
}

function linkPostPublish(selector) {
    $(selector).click(function () {
        var object = $(this);
        post_id = encodeURIComponent(object.attr("id"));
        $.ajax({
            type: "POST",
            url: "../../../publish_post.php",
            data: "publish=1&id=" + post_id,
            success: function (html) {
                if (html == 'true') {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html + '</div>');
                }
            }
        });
    });
}

function linkPostDelete(selector) {
    $(selector).click(function () {
        var object = $(this);
        post_id = encodeURIComponent(object.attr("id"));
        bootbox.confirm("Are you sure you want to delete your post?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: "../../../delete_post.php",
                    data: "id=" + post_id,
                    success: function (html) {
                        if (html == 'true') {
                            window.location.reload();
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html + '</div>');
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
        post_id = encodeURIComponent(object.attr("id"));
        bootbox.confirm("Are you sure you want to delete your comment?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: "../../../includes/delete_comment.php",
                    data: "id=" + post_id,
                    success: function (html) {
                        if (html == 'true') {
                            window.location.reload();
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html + '</div>');
                        }
                    }
                });
            }
        });
    });
}
