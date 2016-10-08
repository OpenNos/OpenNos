/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF.MySQL
{
    public class Mail
    {
        #region Properties

        public byte Amount { get; set; }

        public DateTime Date { get; set; }

        [MaxLength(255)]
        public string EqPacket { get; set; }

        public bool IsOpened { get; set; }

        public bool IsSenderCopy { get; set; }

        public virtual Item Item { get; set; }

        public short? ItemVNum { get; set; }

        [Key]
        public long MailId { get; set; }

        [MaxLength(255)]
        public string Message { get; set; }

        public virtual Character Receiver { get; set; }

        public long ReceiverId { get; set; }

        public virtual Character Sender { get; set; }

        public byte SenderClass { get; set; }

        public byte SenderGender { get; set; }

        public byte SenderHairColor { get; set; }

        public byte SenderHairStyle { get; set; }

        public long SenderId { get; set; }

        public short SenderMorphId { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        #endregion
    }
}