#pragma once
#include <string>

class AuthController {
public:
    bool ValidateUser(std::string& username);
};
