#include "AuthController.h"

bool AuthController::ValidateUser(std::string& username)
{
    printf("Login from %s. Granted\n", username.c_str());
    return true;
}
