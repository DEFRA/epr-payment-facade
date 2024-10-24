async function getResubmissionFee() {
    const regulator = document.getElementById('resubmissionRegulator').value;
    const resultContainer = document.getElementById('resubmissionFeeResult');

    if (!regulator) {
        resultContainer.textContent = '';
        resultContainer.style.display = "none";
        return;
    }

    try {
        const response = await fetch(`/api/v1/producer/resubmission-fee?regulator=${regulator}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const data = await response.json();
            resultContainer.textContent = `Resubmission Fee: £${formatCurrency(data)}`;
            resultContainer.style.display = "block";
        } else {
            const errorData = await response.json();
            console.error('Error fetching resubmission fee:', errorData);

            let errorMessage = 'Failed to fetch resubmission fee.';
            if (errorData.errors) {
                errorMessage = Object.values(errorData.errors).flat().join('\n');
            } else if (errorData.detail) {
                errorMessage = errorData.detail;
            }

            alert(`Error: ${errorMessage}`);
        }

    } catch (error) {
        console.error('Network error:', error);
        alert('Network error occurred.');
    }
}

function formatCurrency(amountInPence) {
    const amountInPounds = amountInPence / 100;
    return amountInPounds.toLocaleString('en-GB', {
        style: 'currency',
        currency: 'GBP'
    }).replace('£', ''); // Remove currency symbol
}
