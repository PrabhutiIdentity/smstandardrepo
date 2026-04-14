/* DemandRecipt.js */

function PrintDemantRecipt() {

    var contents = document.getElementById("divList").innerHTML;

    var frame1 = document.createElement('iframe');
    frame1.name = "frame1";
    frame1.style.cssText = "position:absolute;top:-1000000px;width:210mm;";
    document.body.appendChild(frame1);

    var frameDoc = frame1.contentWindow
        ? frame1.contentWindow.document
        : frame1.contentDocument;

    frameDoc.open();
    frameDoc.write('<!DOCTYPE html>');
    frameDoc.write('<html><head>');
    frameDoc.write('<meta charset="UTF-8">');
    frameDoc.write('<meta name="viewport" content="width=device-width, initial-scale=1.0">');
    frameDoc.write('<title>Demand Receipt</title>');

    /* Base reset */
    frameDoc.write('<style>');
    frameDoc.write('*{box-sizing:border-box;margin:0;padding:0;}');
    frameDoc.write('body{background:white;font-family:"Segoe UI",Arial,sans-serif;}');
    frameDoc.write('</style>');

    /* Load main CSS */
    frameDoc.write('<link rel="stylesheet" href="/css/DemandRecipt.css" type="text/css">');

    /* Load print CSS */
    frameDoc.write('<link rel="stylesheet" href="/css/DemandRecipt.print.css" media="print" type="text/css">');

    frameDoc.write('</head><body>');
    frameDoc.write(contents);
    frameDoc.write('</body></html>');
    frameDoc.close();

    /* Wait for CSS to load then print */
    var printFrame = window.frames["frame1"];
    var checkReady = setInterval(function () {
        try {
            var styleSheets = frameDoc.styleSheets;
            if (styleSheets && styleSheets.length >= 2) {
                clearInterval(checkReady);
                printFrame.focus();
                printFrame.print();
                setTimeout(function () {
                    document.body.removeChild(frame1);
                }, 1000);
            }
        } catch (e) {
            clearInterval(checkReady);
            setTimeout(function () {
                printFrame.focus();
                printFrame.print();
                setTimeout(function () {
                    document.body.removeChild(frame1);
                }, 1000);
            }, 1500);
        }
    }, 200);

    /* Fallback timeout 4 seconds */
    setTimeout(function () {
        clearInterval(checkReady);
        printFrame.focus();
        printFrame.print();
        setTimeout(function () {
            if (document.body.contains(frame1)) {
                document.body.removeChild(frame1);
            }
        }, 1000);
    }, 4000);
}
