function generateGuid() {
    const mask = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx';
    const array = new Uint8Array(mask.length);
    const crypto = window.crypto || window.msCrypto;
    crypto.getRandomValues(array);

    return mask.replace(/[xy]/g, function (c, i) {
        const r = array[i] % 16;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function setGuids() {
    document.getElementById('userId').value = generateGuid();
    document.getElementById('organisationId').value = generateGuid();
}

async function initiatePayment() {
    const userId = document.getElementById('userId').value;
    const organisationId = document.getElementById('organisationId').value;
    const regulator = document.getElementById('regulator').value;
    const amount = document.getElementById('amount').value;
    const reference = document.getElementById('reference').value;

    const requestData = {
        userId: userId,
        organisationId: organisationId,
        regulator: regulator,
        amount: parseFloat(amount),
        reference: reference
    };

    console.log('Request Data:', requestData);

    try {
        const response = await fetch('/api/v1/online-payments', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData),
            redirect: 'follow'
        });

        if (response.ok) {
            const text = await response.text();
            document.open();
            document.write(text);
            document.close();
        } else {
            const errorData = await response.json();
            console.error('Error initiating payment:', errorData);

            let errorMessage = 'Failed to initiate payment.';
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

window.onload = function () {
    setGuids();
};
