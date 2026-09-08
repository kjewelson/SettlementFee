You are reviewing a pull request in this repository. Compare the current

branch against the base branch and review only the changed files.



Focus on:

\- Correctness and logic errors, especially in fee/settlement calculations

&#x20; (rounding, currency handling, off-by-one errors).

\- Security issues (hardcoded secrets, unsafe SQL/queries, unvalidated input).

\- Readability and maintainability (naming, dead code, overly complex methods).

\- Missing or inadequate tests for changed logic.



Do not review formatting-only or whitespace-only changes.



Write your findings as a single markdown comment suitable for posting

directly on the pull request:

\- Start with a one-line summary verdict (e.g. "Looks good", "Needs changes

&#x20; before merge", "Minor suggestions only").

\- List issues grouped by severity: Blocking, Important, Minor/Nit.

\- For each issue, reference the file and approximate line, explain the

&#x20; problem, and suggest a concrete fix.

\- If you find no issues, say so briefly and confirm what you checked.



Keep the review concise and actionable. Do not modify any files.

