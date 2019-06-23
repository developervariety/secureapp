<?php

use App\Controller\Website\Authentication;

$container["Authentication"] = function ($container) {
    return new Authentication($container);
};