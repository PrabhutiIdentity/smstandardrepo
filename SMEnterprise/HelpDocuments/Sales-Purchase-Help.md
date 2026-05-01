# Sales / Purchase Help Document

This help document is for school staff who use `Sale/Purchase` module.

## Module Purpose

`Sale/Purchase` module is used for:

- stock purchase entry
- sale entry for student/employee
- stock allocation without sale
- balance payment receive after partial sale
- invoice print
- payment receipt print
- sales due list report
- sales report

## Main Menu Path

Open:

- `Sale/Purchase -> Transactions`
- `Sale/Purchase -> New Transaction`
- `Sale/Purchase -> Sales Due List Report`
- `Sale/Purchase -> Sales Report`

## Screen 1: Transactions

Screen name: `Stock Management`

Use this screen to:

- choose `Stock Type`: `Purchase`, `Sale`, or `Allocate (No Sale)`
- set `Start Date` and `End Date`
- click search icon to load transactions
- open any transaction with `View`
- print `Invoice`
- print `Payment Receipt`

Suggested screenshot:
- top filter area with `Stock Type`, `Start Date`, `End Date`
- transaction table with `Paid`, `Due`, `Payment`, `Status`

## Screen 2: New Transaction / Stock Details

Screen name: `Transaction Details`

Use this screen to create or update transaction.

### Important fields

- `Transaction Type`
  - `Purchase`: stock bought from vendor
  - `Sale`: item sold to student/employee
  - `Allocate (No Sale)`: stock issued without payment
- `Receiver Type`
  - `Student`
  - `Employee`
- `Transaction Date`
- `Status`
  - `Valid`
  - `Cancelled`
- `Remark`

### For student sale

Select:

- `Session`
- `Class`
- `Section`
- `Student`

### Stock Details

In `Stock Details` section:

- click `Add New Stock Item`
- choose `Product`
- enter `Quantity`
- check `MRP`, `Price`, and `GST`
- click `Add`

### Payment Details

For sale transaction, payment section shows:

- `Amount Already Paid`
- `Payment Date`
- `Payment Mode`
- `Reference No`
- `This Time Payment Amount`
- `Total Amount`
- `Total Paid`
- `Due Amount`
- `Payment Status`

### Payment modes

Examples:

- `Cash Payment`
- `Cheque Payment`
- `NEFT Payment`
- `Google Pay`
- `PayTm`
- `PhonePe`
- `Other Payment`

### Reference No use

Use `Reference No` according to payment mode:

- cash: cash note / voucher no / short remark
- cheque: cheque number
- bank transfer: NEFT or bank reference
- online: UPI or transaction reference

## How To Create Sale Entry

1. Open `Sale/Purchase -> New Transaction`.
2. Select `Transaction Type = Sale`.
3. Select receiver details.
4. Add item in `Stock Details`.
5. Enter payment details.
6. Click `Save` or `Pay Now`.

### Full payment case

If full amount is received:

- `Due Amount` becomes `0`
- `Payment Status` becomes `Paid`
- invoice and receipt can be printed

### Partial payment case

If only part amount is received:

- enter only received amount in `This Time Payment Amount`
- `Due Amount` stays pending
- `Payment Status` becomes `Partial`
- later balance can be received again

## How To Receive Balance Amount

You can receive balance from:

- `Sales Due List Report`
- `Sales Report`
- `Transactions -> View`

Steps:

1. Open the sale.
2. Check `Due Amount`.
3. Enter amount in `This Time Payment Amount`.
4. Fill `Payment Mode` and `Reference No`.
5. Click `Pay Now`.

Result:

- new payment receipt is generated
- payment history is updated
- invoice shows updated paid and due amount

## Payment History

In `Stock Details`, `Payment History` table shows:

- `Receipt No`
- `Date`
- `Mode`
- `Reference No`
- `Received`
- `Total Paid`
- `Balance`
- `Status`
- `Cancel Date`

Use this section to:

- print any payment receipt
- check cancelled receipt
- cancel latest active receipt if needed

## Receipt Cancellation Rules

Important rule:

- only `latest active receipt` should be cancelled first

Why:

- this keeps payment history and due calculation in sequence

After cancel:

- same receipt number remains in history
- status becomes `Cancelled`
- cancel date is shown
- sale can become payable again if balance is restored

## Printing

### Invoice print

Available from:

- `Transactions`
- `Sales Report`
- `Stock Details`

### Payment receipt print

Available from:

- `Transactions`
- `Sales Report`
- `Stock Details -> Payment History`

If browser asks for popup permission:

- allow popups for your school site / localhost domain

## Screen 3: Sales Due List Report

Use this report to see all pending dues.

Main columns:

- `Invoice`
- `Date`
- `Customer`
- `Products`
- `Amount`
- `Paid`
- `Balance`
- `Last Payment`

Action:

- `Receive Balance`

Use this report when cashier wants to collect pending sale dues quickly.

## Screen 4: Sales Report

Use this report for date-wise and product-wise sales checking.

Filters:

- `From`
- `To`
- `Product`

Main columns:

- `Invoice`
- `Date`
- `Customer`
- `Products`
- `Qty`
- `Amount`
- `Paid`
- `Balance`
- `Status`
- `Last Receipt No`

Actions:

- `Print Invoice`
- `Print Receipt`
- `Receive Balance`
- `View`

## Daily Working Flow

Suggested daily flow:

1. Create sale / purchase entry.
2. Print invoice or receipt if needed.
3. Check `Sales Due List Report` for pending dues.
4. Receive balance payments.
5. Use `Sales Report` for review.

## Common Mistakes To Avoid

- do not enter full amount again during balance payment; enter only current received amount
- do not cancel old receipt before latest active receipt
- always fill correct `Reference No` for bank or online payment
- verify `Due Amount` before collecting payment

## Quick Support Notes

If something looks wrong:

- check whether correct `Student` or `Employee` is selected
- confirm `Payment Mode` and `Reference No`
- check `Payment History` before cancelling or re-collecting payment
- use `Sales Report` and `Sales Due List Report` for verification
