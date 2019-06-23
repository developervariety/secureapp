<?php


namespace App\Validation;

use Respect\Validation\Exceptions\NestedValidationException;

class Validator
{
    protected $errors;

    public function validate($request, array $rules)
    {
        foreach ($rules as $field => $rule) {

            try {
                $rule->assert($request->getParam($field));
            } catch (NestedValidationException $e) {
                $this->errors[$field] = $e->getMessages();
            }
        }

        return $this;
    }

    public function errors()
    {
        return $this->errors;
    }

    public function failed()
    {
        return !empty($this->errors);
    }
}