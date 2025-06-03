// Extract the token from the URL query string
const params = new URLSearchParams(window.location.search);
const token = params.get("token");

// Get the result element where we'll show messages
const resultElement = document.getElementById("result");

// Check if the token exists
if (!token) {
    resultElement.innerText = "Error: No token found in the URL.";
} else {
    // Call the backend API to confirm the email
    fetch('https://localhost:5001/api/auth/confirm-email?token=${token}')
        .then(response => {
            if (!response.ok) {
                throw new Error("Server returned an error.");
            }
            return response.text(); // or response.json() depending on your backend
        })
        .then(data => {
            resultElement.innerText = data || "Email confirmed successfully.";
        })
        .catch(error => {
            resultElement.innerText = "Error confirming email: " + error.message;
        });
}
