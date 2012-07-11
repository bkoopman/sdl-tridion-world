Type.registerNamespace("Social.Commands");

Social.Commands.ToDo = function Commands$ToDo(name) {
    Type.enableInterface(this, "Social.Commands.ToDo");
    this.addInterface("Tridion.Cme.Command", [name || "ToDo"]);
};

Social.Commands.ToDo.prototype._isAvailable = function ToDo$_isAvailable(selection, pipeline) {
    return false;
};

Social.Commands.ToDo.prototype._isEnabled = function ToDo$_isEnabled(selection, pipeline) {
    return this._isAvailable(selection, pipeline);
};

Social.Commands.ToDo.prototype._execute = function ToDo$_execute(selection, pipeline) {
    // Functionality still to be implemented
    alert("Functionality still to be implemented");
};