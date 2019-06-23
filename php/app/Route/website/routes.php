<?php

$app->get("/login", "Authentication:login")->setName("login");