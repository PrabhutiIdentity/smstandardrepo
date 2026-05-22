/* =====
   student.js  —  Student Details  |  All tab interactions
   Drop into ~/Scripts/student.js
   Requires: jQuery, daterangepicker (loaded by layout)
   Covers:
     · Tab switching
     · Personal tab  (gender, image preview, save feedback)
     · Parent tab    (lookup)
     · Session modal (open, close, reload, all partial callbacks)
     · Transport modal (open, route/vehicle/stop cascade)
     · Hostel, Custom Fee helpers
   ===== */


/* ── TAB SWITCHING ─────────────────────────────────────────── */
function sdSwitchTab(name, btn) {
    $('.sd-panel').removeClass('active');
    $('.sd-tab-btn').removeClass('active');
    $('#sdTab-' + name).addClass('active');
    $(btn).addClass('active');
}


/* ── GENDER TOGGLE ─────────────────────────────────────────── */
function sdSelectGender(val, btn) {
    $('.sd-gender-opt').removeClass('male female');
    $(btn).addClass(val == 0 ? 'male' : 'female');
    $('#hdnStudentGender').val(val);
}


/* ── IMAGE PREVIEW ─────────────────────────────────────────── */
function sdPreviewImage(input, imgId) {
    if (!input.files || !input.files[0]) return;
    var file = input.files[0];
    if (file.size / 1024 > 5000) {
        ShowNotification('Large Image', 'Image must be less than 5MB.', 'error');
        input.value = '';
        return;
    }
    var reader = new FileReader();
    reader.onload = function (e) {
        /* Show image, hide fallback */
        var $img = $('#' + imgId);
        if ($img.length === 0) {
            /* Image tag doesn't exist yet — create it */
            $('#sdPhotoFrameWrap').prepend('<img id="' + imgId + '" style="position:absolute;top:0;left:0;width:100%;height:100%;object-fit:cover" />');
            $img = $('#' + imgId);
        }
        $img.attr('src', e.target.result).show();
        $('#sdPhotoFallback').hide();
        /* Update hidden field */
        $('#hdnImageName').val(file.name);
    };
    reader.readAsDataURL(file);
}


/* ── SAVE BUTTON FEEDBACK ──────────────────────────────────── */
function sdSaveStart(btn) {
    var $b = $(btn);
    $b.data('orig', $b.html()).prop('disabled', true)
      .html('<svg width="14" height="14" viewBox="0 0 14 14" fill="none" style="animation:sdSpin .7s linear infinite"><circle cx="7" cy="7" r="5" stroke="rgba(255,255,255,.35)" stroke-width="1.5"/><path d="M7 2a5 5 0 015 5" stroke="white" stroke-width="1.5" stroke-linecap="round"/></svg> Saving...');
}
function sdSaveDone(btn) {
    var $b = $(btn);
    $b.html('<svg width="14" height="14" viewBox="0 0 14 14" fill="none"><path d="M2 7l3.5 3.5L12 3" stroke="white" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/></svg> Saved!')
      .css('background', '#0F6E56');
    setTimeout(function () {
        $b.html($b.data('orig')).prop('disabled', false).css('background', '');
    }, 2000);
}


/* ── CONFIRM DELETE ────────────────────────────────────────── */
function sdConfirmDelete(form) {
    if (confirm('Remove this entry? This may affect fee records.')) {
        $(form).submit();
    }
}


/* ── PARENT LOOKUP ─────────────────────────────────────────── */
function sdLookupParent() {
    var val = $.trim($('#sdParentIDDisplay').val());
    if (!val) {
        ShowNotification('Parent ID', 'Please enter a Parent ID to search.', 'info');
        return;
    }
    /* Visual feedback — disable button while fetching */
    var $btn = $('.sd-lookup .sd-btn');
    var origHtml = $btn.html();
    $btn.prop('disabled', true).html('Searching…');

    $.ajax({
        type: 'POST',
        url: '/Account/GetParentDetailsOnParentID/' + val,
        success: function (data) {
            $btn.prop('disabled', false).html(origHtml);

            /* Update display ID and hidden ParentID */
            $('#sdParentIDDisplay').val(data.ParentSID)
                .css({ 'border-color': '#5DCAA5', 'background': '#f5fdf9' });
            $('#hdnParentID').val(data.ParentID);

            /* Father fields */
            $('#sdFatherName').val(data.FatherName);
            $('#sdFatherAadhaar').val(data.FatherAadhaar);
            $('#sdFatherOccupation').val(data.FatherOccupationID);
            $('#sdFatherDOB').val(data.FatherDOB);
            $('#sdFatherMobile').val(data.FatherMobileNo);
            $('#sdFatherEmail').val(data.FatherEmailID);
            $('#sdFatherBloodGroup').val(data.FatherBloodGroup);
            $('#sdFatherEducation').val(data.FatherEducationID);
            $('#sdLandline').val(data.HomeLandLineNo);

            /* Mother fields */
            $('#sdMotherName').val(data.MotherName);
            $('#sdMotherAadhaar').val(data.MotherAadhaar);
            $('#sdMotherOccupation').val(data.MotherOccupationID);
            $('#sdMotherDOB').val(data.MotherDOB);
            $('#sdMotherMobile').val(data.MotherMobileNo);
            $('#sdMotherEmail').val(data.MotherEmailID);
            $('#sdMotherBloodGroup').val(data.MotherBloodGroup);
            $('#sdMotherEducation').val(data.MotherEducationID);
            $('#sdParentAccessCard').val(data.AccessCardNo);

            /* Address fields */
            $('#sdParentHouseNo').val(data.Padd_HouseNo);
            $('#sdParentStreet').val(data.Padd_Street);
            $('#sdParentArea').val(data.Padd_Area);
            $('#sdParentSector').val(data.Padd_Sector);
            $('#sdParentDistrict').val(data.Padd_District);
            $('#sdParentState').val(data.Padd_State);
            $('#sdParentPin').val(data.Padd_PinCode);
            $('#sdParentCountry').val(data.Padd_Country);

            ShowNotification('Parent Found', 'Parent details loaded. Review and save.', 'success');
        },
        error: function () {
            $btn.prop('disabled', false).html(origHtml);
            $('#sdParentIDDisplay')
                .css({ 'border-color': '#E24B4A', 'background': '#FCEBEB' });
            $('#hdnParentID').val(0);
            ShowNotification('Not Found', 'No parent found with this ID. Check and try again.', 'error');
        }
    });
}


/* =====
   SESSION MODAL
   Route : /Account/EditSessionDetails/{StudentSessionUID}/{StudentID}
   Modal : #mdlEditSession  (partial uses this id directly)

   _SessionListPartial calls:
     AddSession(sender)      → sender.id is not used; opens new (uid=0)
     EditSession(sender)     → sender is the <a> tag, closest <tr>
                               has a hidden input with StudentSessionUID
   ===== */
function sdOpenSession(studentSessionUID, studentId) {
    var $modal   = $('#mdlEditSession');
    var $loader  = $modal.find('.sd-modal-loader');
    var $content = $modal.find('.sd-modal-content');

    $content.html('');
    $loader.show();
    $modal.modal('show');

    $.ajax({
        type: 'POST',
        url: '/Account/EditSessionDetails/' + studentSessionUID + '/' + studentId,
        cache: false,
        processData: false,
        contentType: false,
        success: function (html) {
            $loader.hide();
            $content.html(html);

            /* Re-init datepickers that partial rendered */
            $content.find('.dateinput').daterangepicker({
                singleDatePicker: true,
                calender_style: 'picker_1'
            }, function (start, end) {
                console.log(start.toISOString(), end.toISOString());
            });
        },
        error: function () {
            $loader.hide();
            $content.html('<div class="sd-modal-err">Could not load session details. Please try again.</div>');
        }
    });
}

/* Called by _SessionListPartial "Add New Session" button
   Original signature: AddSession(sender) where sender.id = 0 */
function AddSession(sender) {
    sdOpenSession(0, _sdStudentId);
}

/* Called by _SessionListPartial "Edit" link
   Original signature: EditSession(sender)
   sender = the Edit <a> tag; StudentSessionUID is in a hidden
   input inside the same <tr> */
function EditSession(sender) {
    var uid = $(sender).closest('tr').find('input[name="StudentSessionUID"], #hdnStudentSessionUID').val()
           || $(sender).closest('tr').find('[data-uid]').data('uid')
           || 0;
    sdOpenSession(uid, _sdStudentId);
}

/* Variant used in some PSchool versions */
function GetEditSession(sender, studentId) {
    var uid = parseInt($(sender).attr('id')) || 0;
    sdOpenSession(uid, studentId || _sdStudentId);
}

/* Close button inside partial calls CancelSessionEdit() */
function CancelSessionEdit() {
    $('#mdlEditSession').modal('hide');
}

/* Reload the session list partial after a successful save */
function sdReloadSessions(studentId) {
    $.ajax({
        type: 'POST',
        url: '/Account/GetStudentSessions/' + studentId,
        success: function (html) {
            $('#divSessionsList').html(html);
        }
    });
}

/* ── SESSION PARTIAL CALLBACKS ─────────────────────────────
   All functions below are called directly from
   _SessionEditPartial.cshtml — they MUST exist on the page.
─────────────────────────────────────────────────────────── */

/* Session dropdown → auto-fill start/end date inputs */
function SchoolSessionChanged(sender) {
    var $opt = $(sender).find('option:selected');
    $('#txtStartDate').val($opt.attr('startDate'));
    $('#txtEndDate').val($opt.attr('endDate'));
}

/* Class dropdown → load sections */
function SessionClassChanged(sender) {
    $('#divOptionalSubjects').html('<label class="badge bg-green">Please select section to load optional subjects…</label>');
    $('#ddlSections').html('<option value="-1">Loading…</option>');
    $.ajax({
        type: 'POST',
        url: '/Account/GetSectionOnClass/' + $(sender).val(),
        cache: false,
        processData: false,
        contentType: false,
        success: function (html) {
            $('#ddlSections').html('<option value="0">--Select--</option>').append(html);
        },
        error: function () {
            $('#ddlSections').html('<option value="0">--Select--</option>');
        }
    });
}

/* Section dropdown → load optional subjects */
function SessionSectionChanged(sender) {
    $('#divOptionalSubjects').html('<label class="badge bg-green OptedSubjects">Loading optional subjects…</label>');
    $.ajax({
        type: 'POST',
        url: '/Account/GetSectionOptionalSubjects/' + $(sender).val(),
        cache: false,
        processData: false,
        contentType: false,
        success: function (data) {
            var html = '';
            for (var i = 0; i < data.length; i++) {
                html += '<label class="badge bg-red OptedSubjects" onclick="OptionalSubjectClicked(this);" title="This subject is not opted">'
                      + '<input type="hidden" id="hdnSSOSID"          name="SubjectsOpted[' + i + '].Extra1" value="0" />'
                      + '<input type="hidden" id="hdnOpSubjectID"     name="SubjectsOpted[' + i + '].ID"     value="' + data[i].ID + '" />'
                      + '<input type="hidden" id="hdnStudentSessionID" name="SubjectsOpted[' + i + '].Name"  value="0" />'
                      + '<input type="hidden" id="hdnOpType"          name="SubjectsOpted[' + i + '].Extra2" value="0" />'
                      + data[i].Name
                      + '</label>';
            }
            $('#divOptionalSubjects').html(html || '<span style="color:#9E9B95;font-size:13px">No optional subjects for this section.</span>');
        },
        error: function () {
            $('#divOptionalSubjects').html('<span style="color:#E24B4A;font-size:13px">Failed to load subjects.</span>');
        }
    });
}

/* Toggle green/red on optional subject badge */
function OptionalSubjectClicked(sender) {
    var $lbl = $(sender);
    if ($lbl.hasClass('bg-green')) {
        $lbl.removeClass('bg-green').addClass('bg-red')
            .attr('title', 'This subject is not opted')
            .find('#hdnOpType').val(0);
        $lbl.find('#hdnSSOSID').val(0);
    } else {
        $lbl.removeClass('bg-red').addClass('bg-green')
            .attr('title', 'Student opted for this subject')
            .find('#hdnOpType').val(1);
    }
}

/* Active/Inactive status toggle → show/hide reason field */
function StatusChanged(sender) {
    var $hdn = $(sender).closest('label').find('#hdnSessionStatus');
    var cur  = $hdn.val();
    $hdn.val(cur == '0' ? '1' : '0');
    if (cur == '1') {
        $('#dvSessionStatus').show();
    } else {
        $('#dvSessionStatus').hide();
    }
}

/* Session form validation — called by partial's Submit button */
function CheckSessionEditForm(sender) {
    if (!$('#ddlSessionEditClass').val() || $('#ddlSessionEditClass').val() == '0') {
        ShowNotification('Class', 'Please select a class.', 'info');
        return false;
    }
    if (!$('#ddlSections').val() || $('#ddlSections').val() == '0') {
        ShowNotification('Section', 'Please select a section.', 'info');
        return false;
    }

    var sessionId        = $('#ddlSchoolSessions').val();
    var studentSessionUID = $('#hdnStudentSessionUID').val();

    var isNewSessionRequest = $('#hdnIsNewSessionRequest').val();

    if (isNewSessionRequest != '1') {
        return true;
    }



    if (studentSessionUID == 0 || studentSessionUID == '') {
        /* Adding new — check duplicate */
        if ($('#divSessionsList').find('.sestr_' + sessionId).length > 0) {
            ShowNotification('Session', 'This session already exists for the student. Please choose a different session.', 'info');
            return false;
        }
    } else {
        /* Editing existing — allow same combo */
        if ($('#divSessionsList').find('.sestr_' + studentSessionUID + '-' + sessionId).length == 0) {
            ShowNotification('Session', 'This session already exists for the student. Please choose a different session.', 'info');
            return false;
        }
    }
    return true;
}


/* =====
   TRANSPORT MODAL
   Route : /Account/EditTransportDetails/{StudentID}/{THChangeID}
   Modal : #sdTransportModal

   _TransportAllocationEditPartial.cshtml calls:
     TransportRouteChanged(this)        → route → vehicles
     TransportRouteVehicleChanged(this) → vehicle → stops
     TransportStatusChanged(this)       → continue toggle
   ===== */
function sdOpenTransport(studentId, thChangeId) {
    var $modal   = $('#sdTransportModal');
    var $loader  = $modal.find('.sd-modal-loader');
    var $content = $modal.find('.sd-modal-content');

    $content.html('');
    $loader.show();
    $modal.modal('show');

    $.ajax({
        type: 'POST',
        url: '/Account/EditTransportDetails/' + studentId + '/' + thChangeId,
        cache: false,
        processData: false,
        contentType: false,
        success: function (html) {
            $loader.hide();
            $content.html(html);
        },
        error: function () {
            $loader.hide();
            $content.html('<div class="sd-modal-err">Could not load transport details. Please try again.</div>');
        }
    });
}

/* ── TRANSPORT PARTIAL CALLBACKS ───────────────────────────── */

/* Route dropdown → load vehicles */
function TransportRouteChanged(sender) {
    $('#ddlTVehicles').html('<option value="-1">Loading…</option>');
    $('#ddlTStops').html('<option value="-1">--Select--</option>');
    $('#divFilledStatus').html('');

    $.ajax({
        type: 'POST',
        url: '/Account/GetRouteVehicles/' + $(sender).val(),
        cache: false,
        processData: false,
        contentType: false,
        success: function (data) {
            $('#ddlTVehicles').html('<option capacity="0" filled="0" value="0">--Select--</option>');
            $.each(data, function (i, v) {
                $('#ddlTVehicles').append(
                    '<option capacity="' + v.Extra2 + '" filled="' + v.Extra1 + '" value="' + v.ID + '">' + v.Name + '</option>'
                );
            });
            $('#divFilledStatus').html('Filled Seats: 0  Capacity: 0');
        },
        error: function () {
            $('#ddlTVehicles').html('<option value="0">--Select--</option>');
        }
    });
}

/* Vehicle dropdown → show capacity + load stops */
function TransportRouteVehicleChanged(sender) {
    var $opt = $(sender).find('option:selected');
    $('#divFilledStatus').html('Filled Seats: ' + ($opt.attr('filled') || 0) + '  Capacity: ' + ($opt.attr('capacity') || 0));

    $('#ddlTStops').html('<option value="-1">Loading…</option>');

    $.ajax({
        type: 'POST',
        url: '/Account/GetVehicleRouteStoppages/' + $(sender).val() + '/' + ($('#hdnTransportStopID').val() || 0),
        cache: false,
        processData: false,
        contentType: false,
        success: function (data) {
            $('#ddlTStops').html('');
            var stops = data.Stops || data;
            var selectedStop = data.StopDetails ? data.StopDetails.StopID : 0;
            $.each(stops, function (i, s) {
                $('#ddlTStops').append(
                    '<option value="' + s.StopID + '" ' + (s.StopID == selectedStop ? 'selected' : '') + '>'
                    + s.AreaName + ' (' + s.Time + ') Rs.' + s.Rate
                    + '</option>'
                );
            });
        },
        error: function () {
            $('#ddlTStops').html('<option value="0">--Select--</option>');
        }
    });
}

/* Continue (Applicable) toggle → show/hide End Date field */
function TransportStatusChanged(sender) {
    var $hdn = $(sender).closest('label').find('#hdnTransportApplicable');
    var cur  = $hdn.val();
    $hdn.val(cur == '0' ? '1' : '0');

    if (cur == '1') {
        /* switching OFF continue → show end date */
        $('#dvEndDate').show();
    } else {
        /* switching ON continue → hide end date */
        $('#dvEndDate').hide();
    }
}


/* =====
   CUSTOM FEE
   ===== */
function sdFeeSessionChange(select) {
    $('.se-pre-con').show();
    $.ajax({
        type: 'POST',
        url: '/Account/StudentSessionCustomFeeDetails/' + $(select).val(),
        success: function (html) {
            $('#sdCustomFeeBody').html(html);
            $('.se-pre-con').hide();
        }
    });
}

/* Custom fee applicable toggle */
function CustomFeeStatusChanged(sender) {
    var $hdn = $(sender).closest('label').find('#hdnCustomFeeApplicable');
    $hdn.val($hdn.val() == '0' ? '1' : '0');
}


/* =====
   HOSTEL
   ===== */
function sdHostelChanged(select) {
    $.ajax({
        type: 'POST',
        url: '/Account/GetHostelFloors/' + $(select).val(),
        success: function (html) { $('#sdHostelFloors').html(html); }
    });
}
function sdHostelFloorChanged(select) {
    $.ajax({
        type: 'POST',
        url: '/Account/GetHostelFloorRooms/' + $('#sdHostelSelect').val() + '/' + $(select).val(),
        success: function (html) { $('#sdHostelRoomsBody').html(html); }
    });
}
function HostalStatusChanged(sender) {
    var $hdn = $(sender).closest('label').find('#hdnHostelApplicable');
    $hdn.val($hdn.val() == '0' ? '1' : '0');
    $('#divHostalChangedDate').show();
}


/* =====
   STUDENT PERSONAL — validation helpers
   (called from @section scripts in the view)
   ===== */
function CheckStudentDetails() {
    var name = $('#txtName').val().replace(/ /g, '').replace(/\./g, '');
    if (name.length < 2) {
        ShowNotification('Student Name', 'Name must be at least 2 characters (spaces & dots excluded).', 'error');
        return false;
    }
    return true;
}
function CheckParentForm() {
    var name = $('#sdFatherName').val().replace(/ /g, '').replace(/\./g, '');
    if (name.length < 4) {
        ShowNotification('Father Name', 'Father name must be at least 4 characters.', 'error');
        return false;
    }
    return true;
}


/* =====
   INIT
   ===== */
$(function () {
    /* Datepickers on the main page (not inside modals — those
       are re-inited after AJAX loads the partial) */
    $('.sd-datepicker').daterangepicker({
        singleDatePicker: true,
        calender_style: 'picker_1'
    });

    /* CSS keyframe for spinner */
    var s = document.createElement('style');
    s.textContent = '@keyframes sdSpin{to{transform:rotate(360deg)}}';
    document.head.appendChild(s);
});
