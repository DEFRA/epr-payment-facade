async function calculateFees() {
    const producerType = document.getElementById('producerType').value;
    const numberOfSubsidiaries = document.getElementById('numberOfSubsidiaries').value;
    const regulator = document.getElementById('ProducerRegulator').value;
    const isProducerOnlineMarketplace = document.getElementById('IsProducerOnlineMarketplace').checked;
    const IsLateFeeApplicable = document.getElementById('IsLateFeeApplicable').checked;
    const applicationReferenceNumber = document.getElementById('ApplicationReferenceNumber').value;
    const noOfSubsidiariesOnlineMarketplace = document.getElementById('NoOfSubsidiariesOnlineMarketplace').value;
    const submissionDate = document.getElementById('SubmissionDate').value;

    const requestData = {
        producerType: producerType,
        numberOfSubsidiaries: parseInt(numberOfSubsidiaries),
        regulator: regulator,
        isProducerOnlineMarketplace: isProducerOnlineMarketplace,
        IsLateFeeApplicable: IsLateFeeApplicable,
        noOfSubsidiariesOnlineMarketplace: parseInt(noOfSubsidiariesOnlineMarketplace),
        applicationReferenceNumber: applicationReferenceNumber,
        submissionDate: new Date(submissionDate)
    };

    console.log('Request Data:', requestData);

    try {
        const response = await fetch('/api/v1/producer/registration-fee', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData),
            redirect: 'follow'
        });

        if (response.ok) {
            const data = await response.json();
            showModal(data);
        } else {
            const errorData = await response.json();
            console.error('Error calculating fees:', errorData);

            let errorMessage = 'Failed to calculate fees.';
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

async function calculateComplianceSchemeFees() {
    const regulatorCode = document.getElementById('regulatorCode').value;
    const applicationRefNumber = document.getElementById('applicationRefNumber').value;
    const submissionDate = document.getElementById('CSSubmissionDate').value;

    const members = [];
    for (let i = 1; i <= 3; i++) {
        const memberElement = document.getElementById(`member${i}`);
        if (memberElement && memberElement.style.display !== 'none') {
            const memberId = document.getElementById(`memberId${i}`).value;
            const memberType = document.getElementById(`memberType${i}`).value;
            const subsidiaryCount = document.getElementById(`subsidiaryCount${i}`).value;
            const isOnlineMarketplace = document.getElementById(`isOnlineMarketplaceMember${i}`).checked;
            const isLateFeeApplicable = document.getElementById(`isLateFeeApplicableMember${i}`).checked;
            const onlineMarketplaceSubsidiaryCount = document.getElementById(`onlineMarketplaceSubsidiaryCount${i}`).value;

            if (memberId && memberType && subsidiaryCount) {
                members.push({
                    MemberId: memberId,
                    MemberType: memberType,
                    IsOnlineMarketplace: isOnlineMarketplace,
                    IsLateFeeApplicable: isLateFeeApplicable,
                    NumberOfSubsidiaries: parseInt(subsidiaryCount),
                    NoOfSubsidiariesOnlineMarketplace: parseInt(onlineMarketplaceSubsidiaryCount),
                });
            }
        }
    }

    const requestData = {
        Regulator: regulatorCode,
        ApplicationReferenceNumber: applicationRefNumber,
        SubmissionDate: new Date(submissionDate),
        ComplianceSchemeMembers: members
    };

    console.log('Request Data:', requestData);

    try {
        const response = await fetch('/api/v1/compliance-scheme/registration-fee', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData),
        });

        if (response.ok) {
            const data = await response.json();
            displayFees(data);
        } else {
            const errorData = await response.json();
            console.error('Error calculating fees:', errorData);
            let errorMessage = 'Failed to calculate fees.';
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

function displayFees(data) {
    const modal = document.getElementById("resultsModal");
    const modalBody = document.getElementById("modalBody");

    const totalFeesTable = `
        <table>
            <thead>
                <tr>
                    <th>Description</th>
                    <th>Amount</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                <tr class="total-fee">
                    <td>Total Fee</td>
                    <td>£${formatCurrency(data.totalFee)}</td>
                    <td></td>
                </tr>
                <tr class="total-fee">
                    <td>Compliance Scheme Registration Fee</td>
                    <td>£${formatCurrency(data.complianceSchemeRegistrationFee)}</td>
                    <td></td>
                </tr>
                <tr class="total-fee">
                    <td>Previous Payment</td>
                    <td>£${formatCurrency(data.previousPayment)}</td>
                    <td></td>
                </tr>
                <tr class="total-fee">
                    <td>Outstanding Payment</td>
                    <td>£${formatCurrency(data.outstandingPayment)}</td>
                    <td></td>
                </tr>
            </tbody>
        </table>
    `;

    const memberFeesTables = data.complianceSchemeMembersWithFees.map(member => {
        const breakdownRows = member.subsidiariesFeeBreakdown.feeBreakdowns.map(fee => `
                <tr class="reg-info">
                    <td>Band Number ${fee.bandNumber} (£${formatCurrency(fee.unitPrice)})</td>
                    <td>£${formatCurrency(fee.totalPrice)}</td>
                </tr>
            `).join('');

        // Add late fee row for each member if applicable
        const lateFeeRow = member.memberLateRegistrationFee ? `
                <tr class="reg-info">
                    <td>Member Late Registration Fee</td>
                    <td>£${formatCurrency(member.memberLateRegistrationFee)}</td>
                </tr>
            ` : '';

        return `
            <h3>Member ID: ${member.memberId}</h3>
            <table>
                <tbody>
                    <tr class="reg-info">
                        <td>Member Registration Fee</td>
                        <td>£${formatCurrency(member.memberRegistrationFee)}</td>
                    </tr>
                    <tr class="reg-info">
                        <td>Member Online Marketplace Fee</td>
                        <td>£${formatCurrency(member.memberOnlineMarketPlaceFee)}</td>
                    </tr>
                    ${lateFeeRow}
                    <tr class="reg-info">
                        <td>Subsidiaries Fee</td>
                        <td>£${formatCurrency(member.subsidiariesFee)}</td>
                    </tr>
                    ${breakdownRows}
                    <tr class="calc-info">
                        <td>Total Member Fee</td>
                        <td>£${formatCurrency(member.totalMemberFee)}</td>
                    </tr>
                    <tr class="calc-info">
                        <td>Total Subsidiaries OMP Fees (£${formatCurrency(member.subsidiariesFeeBreakdown.unitOMPFees)})</td>
                        <td>£${formatCurrency(member.subsidiariesFeeBreakdown.totalSubsidiariesOMPFees)}</td>
                    </tr>
                </tbody>
            </table>
        `;
    }).join('');

    modalBody.innerHTML = `
        ${totalFeesTable}
        <h2>Member Fees Breakdown</h2>
        ${memberFeesTables}
    `;

    modal.style.display = "block";
}


function closeModal() {
    document.getElementById("resultsModal").style.display = "none";
}

function addMember() {
    for (let i = 1; i <= 3; i++) {
        const memberElement = document.getElementById(`member${i}`);
        if (memberElement && memberElement.style.display === 'none') {
            memberElement.style.display = 'block';
            break;
        }
    }
}

function formatCurrency(amountInPence) {
    const amountInPounds = amountInPence / 100;
    return amountInPounds.toLocaleString('en-GB', {
        style: 'currency',
        currency: 'GBP'
    }).replace('£', ''); // Remove currency symbol
}

function showModal(data) {
    const modal = document.getElementById("resultsModal");
    const modalBody = document.getElementById("modalBody");

    const producerRegistrationFee = `
                                    <tr class="reg-info">
                                        <td>Producer Registration Fee</td>
                                        <td>£${formatCurrency(data.producerRegistrationFee)}</td>
                                    </tr>`;

    const producerOnlineMarketPlaceFee = `
                                    <tr class="reg-info">
                                        <td>Producer Online Marketplace Fee</td>
                                        <td>£${formatCurrency(data.producerOnlineMarketPlaceFee)}</td>
                                    </tr>`;

    const producerLateRegistrationFee = `
                                    <tr class="reg-info">
                                        <td>Producer Late Registration Fee</td>
                                        <td>£${formatCurrency(data.producerLateRegistrationFee)}</td>
                                    </tr>`;

    const breakdownRows = data.subsidiariesFeeBreakdown.feeBreakdowns.map(fee => `
                                    <tr class="reg-info">
                                        <td>Band Number ${fee.bandNumber} (£${formatCurrency(fee.unitPrice)})</td>
                                        <td>£${formatCurrency(fee.totalPrice)}</td>
                                    </tr>
                                `).join('');

    const totalSubsidiariesOMPFees = `
                                <tr class="calc-info">
                                    <td>Total Subsidiaries OMP Fees (£${formatCurrency(data.subsidiariesFeeBreakdown.unitOMPFees)})</td>
                                    <td></td>
                                    <td>£${formatCurrency(data.subsidiariesFeeBreakdown.totalSubsidiariesOMPFees)}</td>
                                </tr>
                            `;

    const subsidiariesFee = `
                            <tr class="calc-info">
                                <td>Subsidiaries Fee</td>
                                <td></td>
                                <td>£${formatCurrency(data.subsidiariesFee)}</td>
                            </tr>
                        `;

    const totalFee = `
                            <tr class="total-fee">
                                <td>Total Fee</td>
                                <td>£${formatCurrency(data.totalFee)}</td>
                                <td></td>
                            </tr>
                        `;

    const previousPayment = `
                            <tr class="total-fee">
                                <td>Previous Payment</td>
                                <td>£${formatCurrency(data.previousPayment)}</td>
                                <td></td>
                            </tr>
                        `;

    const outstandingPayment = `
                            <tr class="total-fee">
                                <td>Outstanding Payment</td>
                                <td>£${formatCurrency(data.outstandingPayment)}</td>
                                <td></td>
                            </tr>
                        `;

    modalBody.innerHTML = `
                            <table>
                               <thead>
                                    <tr>
                                        <th>Description</th>
                                        <th>Amount</th>
                                        <th></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${producerRegistrationFee}
                                    ${producerOnlineMarketPlaceFee}
                                    ${producerLateRegistrationFee}
                                    ${breakdownRows}
                                    ${totalSubsidiariesOMPFees}
                                    ${subsidiariesFee}
                                    ${totalFee}
                                    ${previousPayment}
                                    ${outstandingPayment}
                                </tbody>
                            </table>
                        `;

    modal.style.display = "block";


}
