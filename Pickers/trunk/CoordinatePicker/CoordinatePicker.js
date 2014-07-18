// assumes XML content structure of:
/* <Content xmlns="namespace">
	<image xmlns:xlink="http://www.w3.org/1999/xlink" xlink:type="simple" xlink:href="tcm:1-123"></image>
	<X>3.14</X>
	<Y>42</Y>
   </Content>
*/

// Thanks to the Tridion Cookbook authors for $diplay examples
// https://code.google.com/p/tridion-practice/wiki/AnguillaSnippets

var args =  window.dialogArguments;
var X, Y;
var img;
var imgPath;
var labelX, labelY;

$(document).ready(function() {

// CustomUrl pop-up client-side selections
	labelX = $("#X");
	labelY = $("#Y");

	if (args) {
		var fields = args.getFields();
		if (fields && fields.length > 0) {
			img = args.getFields()[0].getValues();
			if (img != null) { // set image if already linked in the Component
				$('#image').attr("src", "/WebUI/Editors/CME/icon.png?uri=" + img);	
			}
		}		
			
		// controller is $display		
		var fb = args.controller.getView().properties.controls.fieldBuilder;
		//if metadata: $display.getView().getMetadataTab().properties.controls.fieldBuilder

		X = fb.getField("X").getValues();
		Y = fb.getField("Y").getValues();		
		
		labelX.val(X);
		labelY.val(Y);
	}

	(function (window, $, undefined) {

		$("#image").click(function(e) {

		var offset = $(this).offset();
		var relativeX = (e.pageX - offset.left);
		var relativeY = (e.pageY - offset.top);

		var XPercent = (relativeX / $(this).width() * 100 );
		var YPercent = (relativeY / $(this).height() * 100);

		labelX.val(XPercent.toFixed(1).toString()); 
		labelY.val(YPercent.toFixed(1).toString()); 
	});

})(window, jQuery);


$(":button#save").click(function() {
	var args = window.dialogArguments;
	if (args) {
		var fields = args.getFields();

		var fb = args.controller.getView().properties.controls.fieldBuilder;
				
		fb.getField("X").setValues([labelX.val()]);
		fb.getField("Y").setValues([labelY.val()]);
		
	}
		window.close();
});

$(":button#cancel").click(function(e) {
	e.preventDefault();
	window.close();
});

});