async function getProducerResubmissionFee() {
    const regulator = document.getElementById('resubmissionRegulator').value;
    const referenceNumber = document.getElementById('referenceNumber').value;
    const resubmissionDate = document.getElementById('resubmissionDate').value;
    const resultContainer = document.getElementById('resubmissionFeeResult');

    // Prepare request data
    const requestData = {
        regulator: regulator,
        referenceNumber: referenceNumber,
        resubmissionDate: new Date(resubmissionDate).toISOString()
    };

    try {
        const response = await fetch('/api/v1/producer/resubmission-fee', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData)
        });

        if (response.ok) {
            const data = await response.json();
            displayProducerResubmissionFee(data); // Show results in the modal
        } else {
            const errorData = await response.json();
            displayServerErrors(errorData); // Handle server-side validation errors
        }
    } catch (error) {
        console.error('Network error:', error);
        alert('Network error occurred.');
    }
}

function displayProducerResubmissionFee(data) {
    const modal = document.getElementById("resubmissionResultsModal");
    const modalBody = document.getElementById("resubmissionModalBody");

    const resultText = `
        <table>
            <thead>
                <tr>
                    <th>Description</th>
                    <th>Amount</th>
                </tr>
            </thead>
            <tbody>
                <tr class="total-fee">
                    <td>Total Resubmission Fee</td>
                    <td>£${formatCurrency(data.totalResubmissionFee)}</td>
                </tr>
                <tr class="total-fee">
                    <td>Previous Payments</td>
                    <td>£${formatCurrency(data.previousPayments)}</td>
                </tr>
                <tr class="total-fee">
                    <td>Outstanding Payment</td>
                    <td>£${formatCurrency(data.outstandingPayment)}</td>
                </tr>
            </tbody>
        </table>
    `;

    modalBody.innerHTML = resultText;
    modal.style.display = "block";
}

// Compliance Scheme Resubmission Function
async function calculateComplianceResubmissionFee() {
    const regulator = document.getElementById('complianceRegulator').value;
    const resubmissionDate = document.getElementById('complianceResubmissionDate').value;
    const referenceNumber = document.getElementById('complianceReferenceNumber').value;
    const memberCount = document.getElementById('complianceMemberCount').value;
    const resultContainer = document.getElementById('complianceResubmissionFeeResult');

    // Clear previous results
    resultContainer.textContent = '';
    resultContainer.style.display = "none";

    // Prepare request data without client-side validation
    const requestData = {
        regulator: regulator,
        resubmissionDate: new Date(resubmissionDate).toISOString(),
        referenceNumber: referenceNumber,
        memberCount: parseInt(memberCount)
    };

    try {
        const response = await fetch('/api/v1/compliance-scheme/resubmission-fees', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData)
        });

        if (response.ok) {
            const data = await response.json();
            displayComplianceResubmissionFee(data);  // Display the response data in the modal
        } else {
            const errorData = await response.json();
            displayServerErrors(errorData);  // Display server errors for testing
        }

    } catch (error) {
        console.error('Network error:', error);
        alert('Network error occurred.');
    }
}

// Function to display server-side errors
function displayServerErrors(errorData) {
    let errorMessage = 'Error occurred: ';

    // Check if the response contains validation errors or a general error
    if (errorData.errors) {
        // Aggregate all the errors
        errorMessage += Object.values(errorData.errors).flat().join('\n');
    } else if (errorData.detail) {
        // Display the detailed error message
        errorMessage += errorData.detail;
    }

    // Display the error message in an alert or in a designated area on the page
    alert(errorMessage);
}

// Display compliance resubmission result in a modal
function displayComplianceResubmissionFee(data) {
    const modal = document.getElementById("complianceResultsModal");
    const modalBody = document.getElementById("complianceModalBody");

    const resultText = `
        <table>
            <thead>
                <tr>
                    <th>Description</th>
                    <th>Amount</th>
                </tr>
            </thead>
            <tbody>
                <tr class="total-fee">
                    <td>Total Resubmission Fee</td>
                    <td>£${formatCurrency(data.totalResubmissionFee)}</td>
                </tr>
                <tr class="total-fee">
                    <td>Previous Payments</td>
                    <td>£${formatCurrency(data.previousPayments)}</td>
                </tr>
                <tr class="total-fee">
                    <td>Outstanding Payment</td>
                    <td>£${formatCurrency(data.outstandingPayment)}</td>
                </tr>
                <tr class="total-fee">
                    <td>Member Count</td>
                    <td>${data.memberCount}</td>
                </tr>
            </tbody>
        </table>
    `;

    modalBody.innerHTML = resultText;
    modal.style.display = "block";
}

// Close modal function
function closeComplianceModal() {
    document.getElementById("complianceResultsModal").style.display = "none";
}

function closeResubmissionModal() {
    document.getElementById("resubmissionResultsModal").style.display = "none";
}


// Utility function for currency formatting (convert from pence to pounds)
function formatCurrency(amountInPence) {
    const amountInPounds = amountInPence / 100;  // Convert pence to pounds
    return amountInPounds.toLocaleString('en-GB', {
        style: 'currency',
        currency: 'GBP'
    }).replace('£', '');  // Remove currency symbol for consistency
}
