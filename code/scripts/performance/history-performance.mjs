#!/usr/bin/env node

import { formatLogMessage, measureRequestTime } from './utils.mjs';

async function runPerformanceTest() {

    const result = await measureRequestTime(`/accounts`);

    if (!result.ok) {
        console.error('Failed to fetch accounts. Stopping test.');
        return;
    }

    const accounts = (await result.response.json()).accounts;
    console.log();
    console.log(`Retrieved ${accounts.length} accounts. Performing history requests`);
    console.log();

    for (const account of accounts) {
        const accountCode = account.accountCode;

        const requestBody = {
            accountCode: accountCode,
            queryDate: '2025-12-31'
        };

        const portfolioResult = await measureRequestTime(`/account/history`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(requestBody),
        });

        console.log(formatLogMessage(
            accountCode,
            portfolioResult.status,
            portfolioResult.statusText,
            portfolioResult.timeElapsed
        ));
    }
}

runPerformanceTest();
