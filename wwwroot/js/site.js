// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//function toggleLight() {
//    var element = document.getElementById("layoutBody")
//    element.ClassList.toggle("light-mode")

//    if (dark) {
//        localStorage.setItem("jamesonLightMode", JSON.stringify(false))
//        console.log("Light Mode Off")
//    } else {
//        localStorage.setItem("jamesonDarkMode", JSON.stringify(true))
//        console.log("Dark mode on")
//    }

//    var buttonElement = document.getElementById("lightIcon")
//    buttonElement.classList.toggle("fa-sun")
//    buttonElement.classList.toggle("far")
//    buttonElement.classList.toggle("fas")
//    buttonElement.classList.toggle("fa-sun")
//}

//function loadLight() {
//    //default is light mode
//    console.log("light mode is ", JSON.parse(localStorage.getItem("jamesonLightMode")))
//    let dark = JSON.parse(localStorage.getItem("jamesonLightMode"))
//    if (dark === null) {
//        localStorage.setItem("jamesonLightMode", JSON.stringify(false))
//    }
//    else if (dark === true) {
//        document.getElementById("layoutBody").classList.add("light-mode")
//    }
//}