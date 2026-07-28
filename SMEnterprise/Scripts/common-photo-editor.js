(function (window, $) {
    'use strict';

    var MAX_BYTES = 50 * 1024, OUT_W = 350, OUT_H = 450, ASPECT = OUT_W / OUT_H;
    var editor = null;

    function ensureEditor() {
        if (editor) return editor;
        $('body').append(
            '<style>.cpe-canvas{display:block;max-width:100%;margin:auto;background:#222;cursor:move;touch-action:none}.cpe-video{width:100%;max-height:55vh;background:#111}</style>' +
            '<div class="modal fade" id="cpeCropModal" tabindex="-1" role="dialog" data-backdrop="static"><div class="modal-dialog"><div class="modal-content">' +
            '<div class="modal-header"><button type="button" class="close cpe-crop-cancel">&times;</button><h4>Crop Photo</h4></div>' +
            '<div class="modal-body"><canvas class="cpe-canvas" width="500" height="500"></canvas><label style="margin-top:10px">Zoom</label><input class="cpe-zoom form-control" type="range" min="30" max="300" value="100"><p class="help-block">Blue crop box ko move/resize karein. Passport photo 350 × 450 px aur automatically 50 KB se kam save hogi.</p></div>' +
            '<div class="modal-footer"><button type="button" class="btn btn-default cpe-crop-cancel">Cancel</button><button type="button" class="btn btn-primary cpe-apply">Apply Crop</button></div></div></div></div>' +
            '<div class="modal fade" id="cpeCameraModal" tabindex="-1" role="dialog" data-backdrop="static"><div class="modal-dialog"><div class="modal-content">' +
            '<div class="modal-header"><button type="button" class="close cpe-camera-close">&times;</button><h4>Take Photo</h4></div>' +
            '<div class="modal-body"><video class="cpe-video" autoplay playsinline></video><div class="cpe-camera-error alert alert-danger" style="display:none"></div></div>' +
            '<div class="modal-footer"><button type="button" class="btn btn-default cpe-camera-close">Cancel</button><button type="button" class="btn btn-primary cpe-capture"><i class="fa fa-camera"></i> Capture</button></div></div></div></div>');

        var canvas = document.querySelector('#cpeCropModal canvas'), ctx = canvas.getContext('2d');
        editor = { canvas: canvas, ctx: ctx, image: null, rect: null, crop: null, zoom: 1, baseScale: 1, callback: null, stream: null, dragging: false, mode: '', lastX: 0, lastY: 0 };

        $('.cpe-zoom').on('input change', function () { editor.zoom = parseInt(this.value, 10) / 100; setRect(); constrain(); draw(); });
        $(canvas).on('mousedown touchstart', startDrag).on('mousemove touchmove', moveDrag);
        $(document).on('mouseup touchend', function () { editor.dragging = false; });
        $('.cpe-crop-cancel').click(function () { resetCrop(); $('#cpeCropModal').modal('hide'); });
        $('.cpe-camera-close').click(function () { stopCamera(); $('#cpeCameraModal').modal('hide'); });
        $('.cpe-capture').click(capture);
        $('.cpe-apply').click(apply);
        return editor;
    }

    function setRect() {
        var e = editor, scale = e.baseScale * e.zoom, w = e.image.width * scale, h = e.image.height * scale;
        e.rect = { x: (e.canvas.width - w) / 2, y: (e.canvas.height - h) / 2, w: w, h: h, scale: scale };
    }
    function resetBox() {
        var r = editor.rect, w = Math.min(r.w, r.h * ASPECT) * .88;
        editor.crop = { x: r.x + (r.w - w) / 2, y: r.y + (r.h - w / ASPECT) / 2, w: w, h: w / ASPECT };
        constrain();
    }
    function constrain() {
        if (!editor.crop) return;
        var c = editor.crop, min = 80, max = Math.min(editor.canvas.width, editor.canvas.height * ASPECT);
        c.w = Math.max(min, Math.min(max, c.w)); c.h = c.w / ASPECT;
        c.x = Math.max(0, Math.min(editor.canvas.width - c.w, c.x));
        c.y = Math.max(0, Math.min(editor.canvas.height - c.h, c.y));
    }
    function draw() {
        var e = editor, c = e.crop, r = e.rect, x = e.ctx;
        if (!e.image || !c) return;
        x.clearRect(0, 0, e.canvas.width, e.canvas.height); x.fillStyle = '#222'; x.fillRect(0, 0, e.canvas.width, e.canvas.height);
        x.drawImage(e.image, r.x, r.y, r.w, r.h); x.fillStyle = 'rgba(0,0,0,.58)';
        x.fillRect(0, 0, e.canvas.width, c.y); x.fillRect(0, c.y, c.x, c.h); x.fillRect(c.x + c.w, c.y, e.canvas.width - c.x - c.w, c.h); x.fillRect(0, c.y + c.h, e.canvas.width, e.canvas.height - c.y - c.h);
        x.strokeStyle = '#20a8ff'; x.lineWidth = 3; x.strokeRect(c.x, c.y, c.w, c.h); x.fillStyle = '#20a8ff'; x.fillRect(c.x + c.w - 9, c.y + c.h - 9, 18, 18);
    }
    function point(event) {
        var r = editor.canvas.getBoundingClientRect(), p = event.touches ? event.touches[0] : event;
        return { x: (p.clientX - r.left) * editor.canvas.width / r.width, y: (p.clientY - r.top) * editor.canvas.height / r.height };
    }
    function startDrag(e) {
        e.preventDefault(); var p = point(e.originalEvent), c = editor.crop;
        if (Math.abs(p.x - c.x - c.w) <= 25 && Math.abs(p.y - c.y - c.h) <= 25) editor.mode = 'resize';
        else if (p.x >= c.x && p.x <= c.x + c.w && p.y >= c.y && p.y <= c.y + c.h) editor.mode = 'move'; else return;
        editor.dragging = true; editor.lastX = p.x; editor.lastY = p.y;
    }
    function moveDrag(e) {
        if (!editor.dragging) return; e.preventDefault();
        var p = point(e.originalEvent), dx = p.x - editor.lastX, dy = p.y - editor.lastY;
        if (editor.mode === 'move') { editor.crop.x += dx; editor.crop.y += dy; } else editor.crop.w += Math.abs(dx) > Math.abs(dy) ? dx : dy * ASPECT;
        constrain(); editor.lastX = p.x; editor.lastY = p.y; draw();
    }
    function openFile(file, callback) {
        ensureEditor();
        if (!file || (file.type && file.type.indexOf('image/') !== 0)) return notify('Please select a valid image.', 'error');
        editor.callback = callback;
        var reader = new FileReader();
        reader.onload = function (event) {
            var image = new Image();
            image.onload = function () { editor.image = image; editor.baseScale = Math.min(500 / image.width, 500 / image.height); editor.zoom = 1; $('.cpe-zoom').val(100); setRect(); resetBox(); draw(); $('#cpeCropModal').modal('show'); };
            image.onerror = function () { notify('Image could not be opened.', 'error'); };
            image.src = event.target.result;
        };
        reader.readAsDataURL(file);
    }
    function openCamera(callback) {
        ensureEditor(); editor.callback = callback; $('.cpe-camera-error').hide(); $('#cpeCameraModal').modal('show');
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) return $('.cpe-camera-error').text('Camera is not supported or HTTPS is required.').show();
        navigator.mediaDevices.getUserMedia({ video: { facingMode: 'user', width: { ideal: 1280 } }, audio: false })
            .then(function (stream) { editor.stream = stream; $('.cpe-video')[0].srcObject = stream; })
            .catch(function () { $('.cpe-camera-error').text('Camera permission nahi mili ya camera available nahi hai.').show(); });
    }
    function stopCamera() {
        if (editor && editor.stream) { editor.stream.getTracks().forEach(function (track) { track.stop(); }); editor.stream = null; }
        if ($('.cpe-video').length) $('.cpe-video')[0].srcObject = null;
    }
    function capture() {
        var video = $('.cpe-video')[0]; if (!editor.stream || !video.videoWidth) return;
        var canvas = document.createElement('canvas'); canvas.width = video.videoWidth; canvas.height = video.videoHeight; canvas.getContext('2d').drawImage(video, 0, 0);
        stopCamera(); $('#cpeCameraModal').modal('hide'); canvas.toBlob(function (blob) { openFile(blob, editor.callback); }, 'image/jpeg', .95);
    }
    function apply() {
        var e = editor, r = e.rect, c = e.crop, output = document.createElement('canvas'); output.width = OUT_W; output.height = OUT_H;
        var x = output.getContext('2d'); x.fillStyle = '#fff'; x.fillRect(0, 0, OUT_W, OUT_H);
        var ix = Math.max(c.x, r.x), iy = Math.max(c.y, r.y), ir = Math.min(c.x + c.w, r.x + r.w), ib = Math.min(c.y + c.h, r.y + r.h);
        if (ir > ix && ib > iy) x.drawImage(e.image, (ix-r.x)/r.scale, (iy-r.y)/r.scale, (ir-ix)/r.scale, (ib-iy)/r.scale, (ix-c.x)*OUT_W/c.w, (iy-c.y)*OUT_H/c.h, (ir-ix)*OUT_W/c.w, (ib-iy)*OUT_H/c.h);
        compress(output, .92, function (blob) {
            if (!blob || blob.size > MAX_BYTES) return notify('Image 50 KB se kam nahi ho saki.', 'error');
            var callback = e.callback; resetCrop(); $('#cpeCropModal').modal('hide'); if (callback) callback(blob);
        });
    }
    function compress(canvas, quality, done) {
        canvas.toBlob(function (blob) {
            if (blob && blob.size <= MAX_BYTES) return done(blob);
            if (quality > .36) return compress(canvas, quality - .08, done);
            if (canvas.width > 320) {
                var smaller = document.createElement('canvas'); smaller.width = Math.round(canvas.width * .82); smaller.height = Math.round(canvas.height * .82);
                smaller.getContext('2d').drawImage(canvas, 0, 0, smaller.width, smaller.height); return compress(smaller, .82, done);
            }
            done(blob);
        }, 'image/jpeg', quality);
    }
    function resetCrop() { if (!editor) return; editor.image = editor.rect = editor.crop = editor.callback = null; }
    function notify(message, type) { if (window.ShowNotification) window.ShowNotification(type === 'error' ? 'Photo Error' : 'Photo', message, type || 'info'); else window.alert(message); }

    function initSingle(selector) {
        $(selector || '.single-photo-editor').each(function () {
            var widget = $(this), input = widget.find('.single-photo-file'), image = $(widget.data('image'));
            function save(blob) {
                var data = new FormData();
                data.append('personType', widget.data('type')); data.append('personID', widget.data('id'));
                data.append('photo', blob, 'photo.jpg');
                widget.find('.single-photo-state').removeClass('text-danger text-success').text('Saving...');
                widget.find('button,.btn').prop('disabled', true);
                $.ajax({ url: widget.data('url'), type: 'POST', data: data, processData: false, contentType: false })
                    .done(function (result) {
                        if (!result || !result.success) return widget.find('.single-photo-state').addClass('text-danger').text((result && result.message) || 'Save failed.');
                        image.attr('src', result.photoUrl + '?v=' + Date.now()).show();
                        $('#hdnImageName').val(result.fileName);
                        widget.find('.single-photo-state').addClass('text-success').text('Saved (' + Math.ceil(blob.size / 1024) + ' KB)');
                        notify('Photo saved successfully.', 'success');
                    })
                    .fail(function () { widget.find('.single-photo-state').addClass('text-danger').text('Network/server error.'); })
                    .always(function () { widget.find('button,.btn').prop('disabled', false); });
            }
            input.change(function () { if (this.files && this.files[0]) openFile(this.files[0], save); this.value = ''; });
            widget.find('.single-photo-camera').click(function () { openCamera(save); });
        });
    }

    window.SMPhotoEditor = { openFile: openFile, openCamera: openCamera, initSingle: initSingle, maxBytes: MAX_BYTES };
    $(function () { initSingle(); });
})(window, window.jQuery);
