<?php

namespace App\Controller;

use Slim\Http\Request;
use Slim\Http\Response;

class Authentication extends Controller
{
    public function login(Request $request, Response $response)
    {
        return $this->view->render($response, "auth/login.twig");
    }

    public function register(Request $request, Response $response)
    {
        return $this->view->render($response, "auth/register.twig");
    }
}