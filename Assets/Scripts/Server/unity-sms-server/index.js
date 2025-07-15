const express = require('express');
const bodyParser = require('body-parser');
const twilio = require('twilio');
const MessagingResponse = twilio.twiml.MessagingResponse;

const app = express();
app.use(bodyParser.urlencoded({ extended: false }));  // parse x-www-form-urlencoded

// In-memory storage for messages (or votes)
let submissions = [];       // for wheel of fortune words
let voteTally = { };        // for votes, e.g. { "A": 0, "B": 0, ... }

// Webhook endpoint for incoming SMS from Twilio
app.post('/sms', (req, res) => {
    const msgBody = req.body.Body?.trim();      // text of the SMS
    const from = req.body.From;                // sender's phone number

    console.log(`Received SMS from ${from}: ${msgBody}`);

    // Simple logic: decide if this is a vote or a submission
    if (isVotingTime && isVoteOption(msgBody)) {
        // Voting round: update vote tally
        let vote = msgBody.toUpperCase();
        voteTally[vote] = (voteTally[vote] || 0) + 1;
    } else {
        // Submission round (or unrecognized vote): treat as suggestion word
        submissions.push(msgBody);
    }

    // Optionally, respond with a confirmation SMS back to user
    const twiml = new MessagingResponse();
    twiml.message("Thanks! Your input was received.");
    res.type('text/xml').send(twiml.toString());
});

// ... (we will add a GET endpoint for Unity below) ...

const PORT = 3000;
app.listen(PORT, () => console.log(`Server listening on port ${PORT}`));
