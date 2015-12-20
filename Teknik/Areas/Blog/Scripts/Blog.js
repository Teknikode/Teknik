$(document).ready(function () {
    $("#blog_submit").click(function () {
        $('#newPost').modal('hide');
        blogID = $("#blog_blogid").val();
        title = $("#blog_title").val();
        post = $("#blog_post").val();
        $.ajax({
            type: "POST",
            url: addPostURL,
            data: AddAntiForgeryToken({ blogID: blogID, title: title, article: post }),
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });

    $('#editPost').on('show.bs.modal', function (e) {
        $("#edit_blog_post").val("");
        postID = $(e.relatedTarget).attr("id");
        $("#edit_blog_postid").val(postID);
        $.ajax({
            type: "POST",
            url: getPostTitleURL,
            data: { postID: postID },
            success: function (html) {
                if (html.result) {
                    $("#edit_blog_title").val(html.result);
                }
            }
        });
        $.ajax({
            type: "POST",
            url: getPostArticleURL,
            data: { postID: postID },
            success: function (html) {
                if (html.result) {
                    $("#edit_blog_post").val(html.result);
                }
            }
        });
    });

    $("#edit_submit").click(function () {
        $('#editPost').modal('hide');
        postID = $("#edit_blog_postid").val();
        title = $("#edit_blog_title").val();
        post = $("#edit_blog_post").val();
        $.ajax({
            type: "POST",
            url: editPostURL,
            data: AddAntiForgeryToken({ postID: postID, title: title, article: post }),
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });

    $("#comment_submit").click(function () {
        $('#newComment').modal('hide');
        postID = $("#post_id").val();
        post = $("#comment_post").val();
        $.ajax({
            type: "POST",
            url: addCommentURL,
            data: AddAntiForgeryToken({ postID: postID, article: post }),
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
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
            url: getCommentArticleURL,
            data: { commentID: commentID },
            success: function (html) {
                if (html.result) {
                    $("#edit_comment_post").val(html.result);
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
            url: editCommentURL,
            data: AddAntiForgeryToken({ commentID: postID, post: post }),
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });

    var imageUpload = $('#upload_image').upload({
        name: 'file',
        action: uploadURL,
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
    blog_id = $(".blog-main").attr("id");
    $.ajax({
        type: "POST",
        url: getPostsURL,
        data: { blogID: blog_id, count: count, startPostID: start },
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
    post_id = $(".post-comments").attr("id");
    $.ajax({
        type: "POST",
        url: getCommentsURL,
        data: { postID: post_id, count: count, startCommentID: start },
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
        post_id = object.attr("id");
        $.ajax({
            type: "POST",
            url: publishPostURL,
            data: AddAntiForgeryToken({ postID: post_id, publish: false }),
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
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
            data: AddAntiForgeryToken({postID: post_id, publish: true }),
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
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
                    data: AddAntiForgeryToken({ postID: post_id }),
                    success: function (html) {
                        if (html.result) {
                            window.location.reload();
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
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
                    data: AddAntiForgeryToken({ commentID: post_id }),
                    success: function (html) {
                        if (html.result) {
                            window.location.reload();
                        }
                        else {
                            $("#top_msg").css('display', 'inline', 'important');
                            $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                        }
                    }
                });
            }
        });
    });
}
