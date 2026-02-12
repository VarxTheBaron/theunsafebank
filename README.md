Intentional Security Vulnerabilities:

1. No HTTPS
HTTPS redirection is commented out in Program.cs
2. No CSRF Protection
No antiforgery tokens on forms
Vulnerable to Cross-Site Request Forgery attacks
3. Plain Text Passwords
Passwords stored directly in database without hashing
Visible in Customer model
4. XSS Vulnerability
Transfer messages use @Html.Raw() - can inject malicious scripts
No input sanitization
5. Weak Session Management
HttpOnly set to false on cookies
Basic session storage without encryption
6. SQL Injection Risk
While EF Core helps prevent this, there's minimal validation
7. No Input Validation
Minimal validation on registration and transfers
No password strength requirements
8. Race Conditions
No database transactions for transfers
Balance checks not atomic
9. No Authorization Checks
Only basic session checks, easily bypassed