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
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    role: string;
}