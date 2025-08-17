#!/usr/bin/env node

import fetch from 'node-fetch';

const baseUrl = 'http://localhost:5100';

/**
 * Format a log message with columns
 * @param {string} path - The API path
 * @param {string} accountCode - Account code
 * @param {number} status - HTTP status code
 * @param {string} statusText - HTTP status text
 * @param {number} timeElapsed - Time elapsed in milliseconds
 * @returns {string} Formatted log message
 */
export function formatLogMessage(accountCode, status, statusText, timeElapsed) {
    const accountCodeColumn = accountCode.padEnd(20);
    const statusColumn = `${status || 'ERROR'} ${statusText || ''}`.padEnd(20);
    const timeColumn = `${timeElapsed}ms`.padStart(8);
    return `${accountCodeColumn}  ${statusColumn}  ${timeColumn}`;
}

/**
 * Measure the time it takes to make an HTTP request
 * @param {string} path - The API path
 * @param {object} options - Fetch options
 * @returns {object} Result object with response, time elapsed and status information
 */
export async function measureRequestTime(path, options = {}) {
    const startTime = Date.now();

    const url = baseUrl + path;


    console.log(`Making request to: ${url}`);
    try {
        const response = await fetch(url, options);
        const endTime = Date.now();
        const timeElapsed = endTime - startTime;

        // DEBUG: Log response JSON - Remove this after debugging
        const clonedResponse = response.clone();
        clonedResponse.json().then(data => {
            console.log('Response JSON:', JSON.stringify(data, null, 2));
        }).catch(err => {
            console.error('Error parsing JSON:', err);
        });

        return {
            response,
            timeElapsed,
            ok: response.ok,
            path,
            status: response.status,
            statusText: response.statusText
        };
    } catch (error) {
        const endTime = Date.now();
        const timeElapsed = endTime - startTime;

        return {
            response: null,
            timeElapsed,
            ok: false,
            path,
            status: null,
            statusText: error.message,
            error: error
        };
    }
}

// Allow configuration of the base URL
export function setBaseUrl(url) {
    if (url) {
        baseUrl = url;
    }
    return baseUrl;
}
