Type.registerNamespace("New.Ui.Command");

New.Ui.Command.Sample = function (name) {
    Type.enableInterface(this, "New.Ui.Command.Sample");
    this.addInterface("Tridion.Cme.Command", ["Sample"]);
    console.log("New UI Sample");
};

New.Ui.Command.Sample.prototype.isAvailable = function (selection, pipeline) {
    console.log("New UI Sample - isAvailable");
    return selection && selection.getProperty("isSEPage");
};

New.Ui.Command.Sample.prototype.isEnabled = function (selection, pipeline) {
    console.log("New UI Sample - isEnabled");
    return selection && selection.getProperty("isSEPage");
};

New.Ui.Command.Sample.prototype._execute = function (selection) {
    alert("This is my sample command");
};