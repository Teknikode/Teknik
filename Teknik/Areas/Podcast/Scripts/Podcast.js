$(document).ready(function () {
    $("#podcast_submit").click(function () {
        $('#newPodcast').modal('hide');
        title = $("#podcast_title").val();
        post = $("#podcast_description").val();
        files = $("#podcast_files").val();
        $.ajax({
            type: "POST",
            url: addPodcastURL,
            data: AddAntiForgeryToken({ title: title, description: post, files: files }),
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

    $('#editPodcast').on('show.bs.modal', function (e) {
        $("#edit_podcast_title").val("");
        $("#edit_podcast_description").val("");
        podcastId = $(e.relatedTarget).attr("id");
        $("#edit_podcast_podcastid").val(podcastId);
        $.ajax({
            type: "POST",
            url: getPodcastTitleURL,
            data: { podcastID: podcastId },
            success: function (html) {
                if (html.result) {
                    $("#edit_podcast_title").val(html.result);
                }
            }
        });
        $.ajax({
            type: "POST",
            url: getPodcastDescriptionURL,
            data: { podcastID: podcastId },
            success: function (html) {
                if (html.result) {
                    $("#edit_podcast_description").val(html.result);
                }
            }
        });
    });

    $("#edit_submit").click(function () {
        $('#editPodcast').modal('hide');
        podcastId = $("#edit_podcast_podcastId").val();
        title = $("#edit_podcast_title").val();
        description = $("#edit_podcast_description").val();
        $.ajax({
            type: "POST",
            url: editPodcastURL,
            data: AddAntiForgeryToken({ podcastId: podcastId, title: title, description: description }),
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
        podcastId = $("#podcastId").val();
        post = $("#comment_post").val();
        $.ajax({
            type: "POST",
            url: addCommentURL,
            data: AddAntiForgeryToken({ podcastId: postID, article: post }),
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
        $("#edit_comment_id").val(commentID);
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
        postID = encodeURIComponent($("#edit_comment_id").val());
        post = encodeURIComponent($("#edit_comment_post").val());
        $.ajax({
            type: "POST",
            url: editCommentURL,
            data: AddAntiForgeryToken({ commentID: postID, article: post }),
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
});

function loadMorePodcasts(start, count) {
    podcastId = $(".podcast-main").attr("id");
    $.ajax({
        type: "POST",
        url: getPodcastsURL,
        data: { podcastId: podcastId, count: count, startPodcastID: start },
        success: function (html) {
            if (html) {
                $(".podcast-main").append(html);
                linkPostDelete('.delete_podcast');
                linkPostPublish('.publish_podcast');
                linkPostUnpublish('.unpublish_podcast');
                linkAudioPlayer('audio');
                $(window).bind('scroll', bindScrollPosts);
            }
        }
    });
}

function loadMoreComments(start, count) {
    post_id = $(".podcast-comments").attr("id");
    $.ajax({
        type: "POST",
        url: getCommentsURL,
        data: { postID: post_id, count: count, startCommentID: start },
        success: function (html) {
            if (html) {
                $(".podcast-comments").append(html);
                linkCommentDelete('.delete_comment');
                $(window).bind('scroll', bindScrollComments);
            }
        }
    });
}

function bindScrollPosts() {
    if ($(window).scrollTop() + $(window).height() > $(document).height() - 100) {
        $(window).unbind('scroll');
        loadMorePodcasts(startPodcast, podcasts);
        startPodcast = startPodcast + podcasts;
    }
}

function bindScrollComments() {
    if ($(window).scrollTop() + $(window).height() > $(document).height() - 100) {
        $(window).unbind('scroll');
        loadMoreComments(startComment, comments);
        startComment = startComment + comments;
    }
}

function linkPodcastUnpublish(selector) {
    $(selector).click(function () {
        var object = $(this);
        podcastId = object.attr("id");
        $.ajax({
            type: "POST",
            url: publishPodcastURL,
            data: AddAntiForgeryToken({ podcastId: podcastId, publish: false }),
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

function linkPodcastPublish(selector) {
    $(selector).click(function () {
        var object = $(this);
        podcastId = object.attr("id");
        $.ajax({
            type: "POST",
            url: publishPodcastURL,
            data: AddAntiForgeryToken({ podcastId: podcastId, publish: true }),
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

function linkPodcastDelete(selector) {
    $(selector).click(function () {
        var object = $(this);
        podcastId = object.attr("id");
        bootbox.confirm("Are you sure you want to delete the podcast?", function (result) {
            if (result) {
                $.ajax({
                    type: "POST",
                    url: deletePodcastURL,
                    data: AddAntiForgeryToken({ podcastId: podcastId }),
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

function linkAudioPlayer(selector) {
    $(selector).audioPlayer(
    {
        classPrefix: 'audioplayer'
    });
}