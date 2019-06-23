<?php

$app->get("/login", "WebsiteAuthentication:login")->setName("login");