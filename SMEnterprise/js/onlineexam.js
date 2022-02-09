function SubmitAns(questionid) {
    debugger;
    var name = GetAnswers(questionid);
    var Answersobj = new Object();
    Answersobj.Name = name;
    Answersobj.ID = questionid;

    if (Answersobj.Name == "") {
        alert("Please select any option to save");
    }
    else
    {
        var questionbankobj = new Object();
        questionbankobj.QuesAnswers = Answersobj;
        questionbankobj.OExamID = $('#ExamID').val();
        questionbankobj.SubAnsID = $('#hdnSubAnsID_' + questionid).val();
        $.ajax(
            {
                url: "/Parent/SubmitAnswer",
                type: "POST",
                data: JSON.stringify(questionbankobj),
                contentType: 'application/json; charset=utf-8',
                success: function (res) {
                    debugger;
                    if (res != null) {
                        $('#hdnSubAnsID_' + questionid).val(res);
                    }
                },
                error: function (e) {
                    alert("errorn");
                }
            });
        SaveAndNext();
    }
}

function SaveAndNext()
{
    debugger;
         if ($(".divs ul li:visible").next().length != 0) {
            $(".divs ul li:visible").next().show().prev().hide();

            var hasnextelement = $(".divs ul li:visible").next().length;
            if (hasnextelement == 0) {
                $("#next").hide();
                $(".divs ul li #Submitanswer").hide();
                $(".divs ul li #submitsheet").show();
            }
            else {
                $("#next").show();
                $(".divs ul li #submitsheet").hide();
                $(".divs ul li #submitans").show();
            }

            var hasprevelement = $(".divs ul li").prev().length;
            if (hasprevelement == 0) {
                $("#prev").hide();
            }
            else {
                $("#prev").show();
            }
        }
        else {
             $(".divs ul li:visible").hide();
             $(".divs ul li:last").show();
             //$('#lastAnswer').html("You have completed your exam, Press Submit Button to submit the sheet or Press Previous button to go for prevoius questions.")
        }
        return false;
 }

function GetAnswers(questionid) {
    debugger;
    var opt = $('#hdnQuestionType_' + questionid).val();
    var answer = '';
    if (opt == 0) {
        if ($('#SCOpetion1_' + questionid).is(':checked') == true) {
            answer = 'A';
        }
        else if ($('#SCOpetion2_' + questionid).is(':checked') == true) {
            answer = 'B';
        }
        else if ($('#SCOpetion3_' + questionid).is(':checked') == true) {
            answer = 'C';
        }
        else if ($('#SCOpetion4_' + questionid).is(':checked') == true) {
            answer = 'D';
        }
    }
    else if (opt == 1) {
        if ($('#SMOpetion1_' + questionid).is(':checked') == true) {
            answer = 'A';
        }
        if ($('#SMOpetion2_' + questionid).is(':checked') == true) {
            if (answer != '') {
                answer = answer + ',';
            }
            answer = answer + 'B';
        }
        if ($('#SMOpetion3_' + questionid).is(':checked') == true) {
            if (answer != '') {
                answer = answer + ',';
            }
            answer = answer + 'C';
        }
        if ($('#SMOpetion4_' + questionid).is(':checked') == true) {
            if (answer != '') {
                answer = answer + ',';
            }
            answer = answer + 'D';
        }
    }
    else {
        answer = $('#txtSubjectiveAnswer_' + questionid ).val();
    }
    return answer;
}