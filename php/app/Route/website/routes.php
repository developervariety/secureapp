<?php

$app->get("/login", "Authentication:login")->setName("login");

$app->get("/register", "Authentication:register")->setName("register");
$app->post("/register", "Authentication:postRegister");