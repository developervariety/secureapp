<?php

namespace App\Controller;

use Requests;
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

    public function postRegister(Request $request, Response $response)
    {
        $headers = array("Accept" => "application/json");
        $options = $request->getParams();

        $request = Requests::post("https://secureapp.developer-variety.com/api/auth/register", $headers, $options);
        $body = json_decode($request->body, true);

        if ($body["status"] == "error") {
            foreach ($body["data"] as $key => $value) {
                foreach ($value as $rule) {
                    // TODO:: display all errors
                    $this->flash->addMessage("multidanger", $rule);
                }
            }
        }

        return $this->view->render($response, "auth/register.twig");
    }
}