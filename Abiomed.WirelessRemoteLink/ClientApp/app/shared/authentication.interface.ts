export interface AuthenticationInterface {
    firstName: string;
    lastName: string;
    fullName: string;
    isSuccess: boolean;
    viewedTermsAndConditions: boolean
    response: string;
    role: string;
}

export interface UserRegistrationInterface {
    FirstName: string;
    LastName: string;
    Email: string;
    Phone: string;
    Roles: Array<string>;
}