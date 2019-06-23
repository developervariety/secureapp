<?php

use App\Controller\Api\Authentication as ApiAuth;
use App\Controller\Website\Authentication as WebsiteAuth;

$container["ApiAuthentication"] = function ($container) {
    return new ApiAuth($container);
};

$container["WebsiteAuthentication"] = function ($container) {
    return new WebsiteAuth($container);
};