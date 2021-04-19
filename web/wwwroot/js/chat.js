"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable send button until connection is established

connection.on("Refresh", function () {
    console.log("Refresh Image");
    var newImage = document.getElementById("render")
    newImage.src = "/test.png?" + new Date().getTime();

});

connection.start().then(function () {
}).catch(function (err) {
    return console.error(err.toString());
});